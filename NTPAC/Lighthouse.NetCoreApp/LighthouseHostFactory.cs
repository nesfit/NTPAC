using System;
using System.Linq;
using Akka.Actor;
using Akka.Configuration;

namespace Lighthouse.NetCoreApp
{
    /// <summary>
    ///     Launcher for the Lighthouse <see cref="ActorSystem" />
    /// </summary>
    public static class LighthouseHostFactory
    {
        public static ActorSystem LaunchLighthouse(String ipAddress = null, Int32? specifiedPort = null)
        {
            var systemName    = "lighthouse";
            var clusterConfig = GetConfig();

            var lighthouseConfig = clusterConfig.GetConfig("lighthouse");
            if (lighthouseConfig != null)
            {
                systemName = lighthouseConfig.GetString("actorsystem", systemName);
            }

            var remoteConfig = clusterConfig.GetConfig("akka.remote");
            ipAddress        = ipAddress     ?? remoteConfig.GetString("dot-netty.tcp.public-hostname") ?? "127.0.0.1"; //localhost as a final default
            var port         = specifiedPort ?? remoteConfig.GetInt("dot-netty.tcp.port");

            if (port == 0)
            {
                throw new ConfigurationException("Need to specify an explicit port for Lighthouse. Found an undefined port or a port value of 0 in App.config.");
            }

            var selfAddress = $"akka.tcp://{systemName}@{ipAddress}:{port}";
            var seeds       = clusterConfig.GetStringList("akka.cluster.seed-nodes");
            if (!seeds.Contains(selfAddress))
            {
                seeds.Add(selfAddress);
            }

            var injectedClusterConfigString = seeds.Aggregate("akka.cluster.seed-nodes = [", (current, seed) => current + @"""" + seed + @""", ");
            injectedClusterConfigString     += "]";

            var finalConfig = ConfigurationFactory.ParseString(
	                                                  $@"akka.remote.dot-netty.tcp.public-hostname = {ipAddress}
														 akka.remote.dot-netty.tcp.port = {port}")
                                                  .WithFallback(ConfigurationFactory.ParseString(injectedClusterConfigString)).WithFallback(clusterConfig);

            return ActorSystem.Create(systemName, finalConfig);
        }

        private static Config GetConfig() =>
	        ConfigurationFactory.ParseString(@"
                    lighthouse{
		                    actorsystem: ""NTPAC-Cluster""
	                    }

                    akka {
	                    actor { 
		                    provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
	                    }
						
	                    remote {
		                    log-remote-lifecycle-events = DEBUG
		                    dot-netty.tcp {
			                    transport-class = ""Akka.Remote.Transport.DotNetty.TcpTransport, Akka.Remote""
			                    applied-adapters = []
			                    transport-protocol = tcp
			                    hostname = ""localhost""
                                public-hostname = ""localhost""
			                    port = 8000
		                    }
	                    }     
											
	                    cluster {
		                    #will inject this node as a self-seed node at run-time
		                    seed-nodes = [""akka.tcp://NTPAC-Cluster@localhost:8000""]
		                    roles = [lighthouse]
	                    }
                    }
            ");
    }
}
