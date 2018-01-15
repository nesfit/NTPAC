using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.Configuration;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.Supervision;
using Akka.Streams.Util;
using Microsoft.Extensions.Logging;
using NTPAC.Actors;
using NTPAC.Common.Interfaces;
using NTPAC.Common.Models;
using NTPAC.Messages;
using PacketDotNet;
using PacketDotNet.IP;
using SharpPcap;

namespace NTPAC.LoadBalancer
{
//    public class LoadBalancerSimple : IPacketIngestor
//    {
//        private readonly ILogger<LoadBalancerSimple> _logger;
//        private readonly IPcapLoader _pcapLoader;
//
//        private readonly Config _systemConfig = ConfigurationFactory.ParseString(@"
//            akka {  
//                actor {
//                    provider = remote
//                    serializers {
//                        messagepack = ""Akka.Serialization.MessagePack.MsgPackSerializer, Akka.Serialization.MessagePack""
//                    }
//                    serialization-bindings {
//                        ""System.Object"" = messagepack
//                    }
//                }
//                remote {
//                    dot-netty.tcp {
//                        port = 0
//                        hostname = localhost
//                    }
//                }
//            }");
//
//        public LoadBalancerSimple(IPcapLoader pcapLoader, ILoggerFactory loggerFactory)
//        {
//            this._pcapLoader = pcapLoader;
//            this._logger     = loggerFactory.CreateLogger<LoadBalancerSimple>();
//        }
//
//        public void OpenPcap(Uri uri)
//        {
//            if (!File.Exists(uri.AbsolutePath))
//            {
//                throw new FileNotFoundException($"File {uri.AbsolutePath} is missing.");
//            }
//
//            using (var system = ActorSystem.Create("NTPAC-LoadBalancer", this._systemConfig))
//            using (var materializer = system.Materializer())
//            {
//                var workerAddress = Address.Parse("akka.tcp://NTPAC-Worker@localhost:7070");
//
//                // TODO Isolate stream's state
//                var l34ConversationActors = new ConcurrentDictionary<L3L4ConversationKey, IActorRef>();
//
//                var packetSource = this.CreatePacketSource(uri);
//
//                var packetParser = CreatePacketParser();
//
//                var packetDistributor = CreatePacketDistributor(l34ConversationActors, system, workerAddress);
//
//                var sw = new Stopwatch();
//                sw.Start();
//
//                packetSource.Via(packetParser).Via(packetDistributor).RunWith(Sink.Ignore<Object>(), materializer).Wait();
//
//                this._logger.LogInformation($"Elapsed: {sw.Elapsed}");
//                this._logger.LogInformation("Shutting down");
//
//                CoordinatedShutdown.Get(system).Run().Wait();
//            }
//        }
//
//        public Task OpenPcapAsync(Uri uri) => throw new NotImplementedException();
//
//        private static Flow<(RawCapture, Packet), Object, NotUsed> CreatePacketDistributor(ConcurrentDictionary<L3L4ConversationKey, IActorRef> l34ConversationActors,
//                                                                                           ActorSystem system,
//                                                                                           Address workerAddress)
//        {
//            var packetDistributor = Flow.Create<ValueTuple<RawCapture, Packet>>().SelectAsync(Environment.ProcessorCount, packetTuple =>
//            {
//                var rawCapture   = packetTuple.Item1;
//                var parsedPacket = packetTuple.Item2;
//
//                if (rawCapture == null || !(parsedPacket?.PayloadPacket is IpPacket ipPacket))
//                {
//                    return Task.FromResult(new Object());
//                }
//
//                var l34Key                     = new L3L4ConversationKey(ipPacket);
//                var remoteL34ConversationActor =
//                    l34ConversationActors.GetOrAdd(
//                        l34Key,
//                        k => system.ActorOf(L34Conversation.Props(l34Key.L3ConversationKey, l34Key.L4ConversationKey)
//                                                           .WithDeploy(Deploy.None.WithScope(new RemoteScope(workerAddress)))));
//                return remoteL34ConversationActor.Ask(new ProcessRawPacketRequest(rawCapture));
//            }).WithAttributes(ActorAttributes.CreateSupervisionStrategy(Deciders.ResumingDecider));
//            return packetDistributor;
//        }
//
//        private static Flow<RawCapture, (RawCapture, Packet), NotUsed> CreatePacketParser() =>
//            Flow.Create<RawCapture>()
//                .SelectAsync(Environment.ProcessorCount, rawPacket => Task.Run(() => (rawPacket : rawPacket, Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data) )))
//                .WithAttributes(ActorAttributes.CreateSupervisionStrategy(Deciders.ResumingDecider));
//
//        private Source<RawCapture, NotUsed> CreatePacketSource(Uri uri)
//        {
//            return Source.UnfoldResource(
//                // Capture opening
//                () =>
//                {
//                    this._pcapLoader.Open(uri);
//                    return this._pcapLoader;
//                },
//                // Capture reading
//                pcapLoader =>
//                {
//                    var rawPacket = pcapLoader.GetNextPacket();
//                    return rawPacket != null ? new Option<RawCapture>(rawPacket) : Option<RawCapture>.None;
//                },
//                // Capture closing
//                pcapLoader => pcapLoader.Close());
//        }
//    }
}
