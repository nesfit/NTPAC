using System;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Configuration;
using NTPAC.Actors;
using NTPAC.Common.Models;
using NTPAC.Messages;

namespace NTPAC.Worker
{
    internal class Program
    {
        private static void Main(String[] args)
        {
            var systemConfig = ConfigurationFactory.ParseString(@"
            akka {
                actor {
                    provider = cluster
                    serializers {
                        messagepack = ""Akka.Serialization.MessagePack.MsgPackSerializer, Akka.Serialization.MessagePack""
                        bytes = ""Akka.Serialization.ByteArraySerializer""
                    }
                    serialization-bindings {
                        ""System.Object"" = messagepack
                         ""System.Byte[]"" = bytes
                    }
                }
                remote {
                    maximum-payload-bytes = 2000000 bytes
                    dot-netty.tcp {
                        port = 0
                        hostname = localhost
                        maximum-frame-size = 2000000b
                        message-frame-size = 2000000b
                        send-buffer-size = 2100000b
                        receive-buffer-size = 2100000b
                    }
                }
                cluster {
                    seed-nodes = [""akka.tcp://NTPAC-Cluster@localhost:8000""]
                    roles = [""worker""]
                }
            }");
            using (var system = ActorSystem.Create("NTPAC-Cluster", systemConfig))
            {
                var region = ClusterSharding.Get(system).Start(Capture.TypeName, Capture.Props(new CaptureInfo("todo")), ClusterShardingSettings.Create(system).WithRole("worker"),
                                                               new RawPacketsMessageExtractor());

                Console.WriteLine($"Press enter to gracefull shutdown ...");
                Console.ReadLine();
                CoordinatedShutdown.Get(system).Run().Wait();
            }

            Console.WriteLine("Done");
        }
    }
}
