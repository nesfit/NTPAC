using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Configuration;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.Util;
using Microsoft.Extensions.Logging;
using NTPAC.Actors;
using NTPAC.Common.Interfaces;
using NTPAC.Common.Models;
using NTPAC.Messages;
using NTPAC.Messages.Sharding;
using NTPAC.Reassembling;
using PacketDotNet;
using PacketDotNet.IP;
using SharpPcap;

namespace NTPAC.LoadBalancer
{
    public class LoadBalancer : IPacketIngestor
    {
        private const Int32 BatchSize = 4000;
        private const Int32 MaxNumberOfNodes = 5;
        private const Int32 MaxNumberOfShards = MaxNumberOfNodes * 10;
        private static readonly TimeSpan BatchFlushInterval = TimeSpan.FromSeconds(10);

        private static readonly Config SystemConfig = ConfigurationFactory.ParseString(@"
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
                    maximum-payload-bytes = 10000000 bytes
                    dot-netty.tcp {
                        port = 9090
                        hostname = localhost
                        maximum-frame-size = 10000000b
                        send-buffer-size = 10000000b
                        receive-buffer-size = 10000000b
                    }
                }
                cluster {
                    seed-nodes = [""akka.tcp://NTPAC-Cluster@localhost:8000""]
                }
            }");

        private readonly ILogger<LoadBalancer> _logger;
        private readonly IPcapLoader _pcapLoader;

        private readonly Stopwatch _totalSw = new Stopwatch();
        private readonly Stopwatch _distributionSw = new Stopwatch();
        
        private readonly Ipv4DefragEngine _ipIpv4DefragEngine = new Ipv4DefragEngine();
        
        //private LeastPacketsAllocationStrategy _shardAllocStrategy = new LeastPacketsAllocationStrategy();

        public LoadBalancer(IPcapLoader pcapLoader, ILoggerFactory loggerFactory)
        {
            this._pcapLoader = pcapLoader;
            this._logger     = loggerFactory.CreateLogger<LoadBalancer>();
        }

        public void OpenPcap(Uri uri) { Task.Run(async () => await this.OpenPcapAsync(uri)).Wait(); }

        public async Task OpenPcapAsync(Uri uri)
        {
            if (!File.Exists(uri.AbsolutePath))
            {
                throw new FileNotFoundException($"File {uri.AbsolutePath} is missing.");
            }

            using (var system = ActorSystem.Create("NTPAC-Cluster", SystemConfig))
            using (var materializer = system.Materializer())
            {
                var clusterProxy = ClusterSharding.Get(system).StartProxy(Capture.TypeName, "worker", new RawPacketsMessageExtractor());

                var packetSource = this.CreatePacketSource(uri);

                var pipeline = this.CreatePipeline(packetSource, clusterProxy);

                this._totalSw.Start();
                if (pipeline != null)
                {
                    var pipelineResult = await pipeline.RunWith(Sink.Aggregate<Int32, Int32>(0, (sum, n) => sum + n), materializer);

                    this._logger.LogInformation($"Elapsed: {this._totalSw.Elapsed}, Real Elapsed: {this._distributionSw.Elapsed}");
                    this._logger.LogInformation($"Distributed packets: {pipelineResult}");
                }
                else
                {
                    this._logger.LogInformation("Pipeline is null.");
                }

                this._logger.LogInformation("Shutting down");

                await ClusterSharding.Get(system).ShardRegion(Capture.TypeName).GracefulStop(TimeSpan.FromSeconds(10));
                await CoordinatedShutdown.Get(system).Run();
            }

            this._logger.LogInformation("Done");
        }


        private Source<RawCapture, NotUsed> CreatePacketSource(Uri uri) =>
            Source.UnfoldResource(
                // Capture opening
                () =>
                {
                    this._pcapLoader.Open(uri);
                    return this._pcapLoader;
                },
                // Capture reading
                pcapLoader =>
                {
                    var rawCapture = pcapLoader.GetNextPacket();
                    return rawCapture != null ? new Option<RawCapture>(rawCapture) : Option<RawCapture>.None;
                },
                // Capture closing
                pcapLoader => pcapLoader.Close());

        private Source<Int32, NotUsed> CreatePipeline(Source<RawCapture, NotUsed> packetSource, IActorRef clusterProxy) =>
            packetSource
                // Parse packets and construct ProcessRawPacketRequest objects with EntityId values set
                // EntityId is calculated from hash of the L3L4ConversationKey
                .SelectAsync(Environment.ProcessorCount, this.ParsePacketAsync)
                // Filter out invalid packets (future feature: channel invalid packets into the logging sink)
                .WhereNot(processRawPacketRequest => processRawPacketRequest is ProcessRawPacketRequestError)
                // Flatout fragment groups
                .SelectMany(FlatOutProcessRawPacketFragmentsRequests)
                // Split stream of packets based on their EntityId into the corresponding substreams
                .GroupBy(MaxNumberOfShards, processRawPacketRequest => processRawPacketRequest.EntityId)
                // Batch packets (by max size and interval) in the substreams
                .GroupedWithin(BatchSize, BatchFlushInterval)
                // Append sequence counter to each batch (for each shard individually)
                .ZipWithIndex()
                // Send each batch to given node
                .SelectAsyncUnordered(Environment.ProcessorCount,
                                      packetBatchWithSeqId => this.SendPacketBatchAsync(packetBatchWithSeqId.Item1, packetBatchWithSeqId.Item2, clusterProxy))
                // Workaround to solve the substream's RunWith NotImplementedException
                .MergeSubstreams() as Source<Int32, NotUsed>;

        private Task<ProcessRawPacketRequest> ParsePacketAsync(RawCapture rawCapture) =>
            Task.Run(() =>
            {
                if (rawCapture == null)
                {
                    return new ProcessRawPacketRequestError("Missing packet payload");
                }

                Packet parsedPacket;
                try
                {
                    parsedPacket = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);   
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    return new ProcessRawPacketRequestError($"Parsing error {ex}");
                }
                
                // Ignore STP, ...
                if (!(parsedPacket?.PayloadPacket is IpPacket ipPacket))
                {
                    return new ProcessRawPacketRequestError("Non-IP packet");
                }

                // Attempt to defragment
                if (ipPacket is IPv4Packet ipv4Packet && Ipv4DefragEngine.Ipv4PacketIsFragmented(ipv4Packet))
                {
                    var defragResult = this._ipIpv4DefragEngine.TryFragmentDefrag(new IPFragment(ipv4Packet, rawCapture));
                    if (defragResult == null)
                    {
                        return ProcessRawPacketFragmentsRequest.EmptyInstance;
                    }
                    var (fragmentedRawCaptures, defragmentedIpv4Packet) = defragResult;
                    var defragmentedL3L4Key = new L3L4ConversationKey(defragmentedIpv4Packet);
                    return new ProcessRawPacketFragmentsRequest(fragmentedRawCaptures.Select(frcr => new ProcessRawPacketRequest(frcr, defragmentedL3L4Key, MaxNumberOfShards)));
                }

                var l3L4Key = new L3L4ConversationKey(ipPacket);
                return new ProcessRawPacketRequest(rawCapture, l3L4Key, MaxNumberOfShards);
            });

        private static IEnumerable<ProcessRawPacketRequest> FlatOutProcessRawPacketFragmentsRequests(ProcessRawPacketRequest processRawPacketRequest)
        {
            if (processRawPacketRequest is ProcessRawPacketFragmentsRequest processRawPacketFragmentsRequest)
            {
                return processRawPacketFragmentsRequest.FragmentRequests;
            }
            // TODO fix the array allocation for non-fragmented packets
            return new ProcessRawPacketRequest[1] {processRawPacketRequest};
        }

        private Task<Int32> SendPacketBatchAsync(IEnumerable<ProcessRawPacketRequest> processRawPacketRequests, Int64 batchSeqId, IActorRef clusterProxy)
        {
            if (!this._distributionSw.IsRunning)
            {
                this._distributionSw.Start();
            }

            var rawPacketRequests = processRawPacketRequests as List<ProcessRawPacketRequest> ?? processRawPacketRequests.ToList();
            if (!rawPacketRequests.Any())
            {
                return Task.FromResult(0);
            }

            // Concrete shardRequestsBatch type needed for faster serialization (using type keys instead of class names)
            var batch    = new ProcessRawPacketBatchRequest(rawPacketRequests, batchSeqId);
            var envelope = new RawPacketsShardEnvelope(rawPacketRequests[0].EntityId, batch);
            var sendTask = clusterProxy.Ask(envelope, TimeSpan.FromSeconds(5));
            return sendTask.ContinueWith(_ => {
                var sentRawPacketRequests = rawPacketRequests.Count;

                // Update shard allocation strategy's stats
                // Use EntityId as ShardId
                //this._shardAllocStrategy.UpadateShardPacketsRecord(envelope.EntityId.ToString(), sentRawPacketRequests);

                return sentRawPacketRequests;
            });
        }
    }
}
