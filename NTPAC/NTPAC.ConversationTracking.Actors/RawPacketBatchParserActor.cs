﻿using System;
using Akka.Actor;
using Akka.Event;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Factories;
using NTPAC.Messages.CaptureTracking;
using NTPAC.Messages.RawPacket;
using PacketDotNet;

namespace NTPAC.ConversationTracking.Actors
{
  public class RawPacketBatchParserActor : ReceiveActor
  {
    private readonly ILoggingAdapter _logger = Context.GetLogger();

    public RawPacketBatchParserActor(ICaptureTrackingActorFactory captureTrackingActorFactory,
                                     IActorRef contractor,
                                     ICaptureInfo captureInfo)
    {
      this.Contractor           = contractor;
      this.CaptureTrackingActor = captureTrackingActorFactory.Create(Context, captureInfo, this.Self);
      this.Become(this.ParsingRawPacketBatches);
    }

    public IActorRef CaptureTrackingActor { get; set; }

    public IActorRef Contractor { get; set; }

    public static Props Props(ICaptureTrackingActorFactory captureTrackingActorFactory,
                              IActorRef contractor,
                              ICaptureInfo captureInfo) =>
      Akka.Actor.Props.Create<RawPacketBatchParserActor>(captureTrackingActorFactory, contractor, captureInfo);

    private void OnCaptureTrackingCompleted(CaptureTrackingCompleted completed)
    {
      this.Contractor.Tell(completed);
      Context.Stop(this.Self);
    }

    private void OnCaptureTrackingComplete(CaptureTrackingComplete complete)
    {
      this.CaptureTrackingActor.Tell(complete);
    }

    private void OnRawPacketBatchRequest(RawPacketBatchRequest rawPacketBatchRequest)
    {
      var ack = new RawPacketBatchAck { MessageId = rawPacketBatchRequest.MessageId};
      this.Contractor.Tell(ack);
   
      foreach (var rawPacket in rawPacketBatchRequest.RawPackets)
      {
        if (rawPacket.RawPacketData == null)
        {
          this._logger.Error("RawPacket has no packet data");
          continue;
        }

        try
        {
          var parsedPacket = Packet.ParsePacket(rawPacket.LinkType, rawPacket.RawPacketData);
          if (!(parsedPacket.PayloadPacket is IPPacket))
          {
            //this._logger.Debug("Ignoring non-IP packet");
            continue;
          }

          var frame = FrameFactory.CreateFromPacket(parsedPacket, rawPacket.DateTimeTicks);
          if (frame.IsValidTransportPacket || frame.IsIpv4Fragmented)
          {
            this.CaptureTrackingActor.Tell(frame);
          }
        }
        catch (Exception e)
        {
          this._logger.Error(e, $"Parsing of a raw packet ({rawPacket}) caused exception.");
        }
      }
    }

    private void ParsingRawPacketBatches()
    {
      this.Receive<RawPacketBatchRequest>(request => this.OnRawPacketBatchRequest(request));
      this.Receive<CaptureTrackingComplete>(complete => this.OnCaptureTrackingComplete(complete));
      this.Receive<CaptureTrackingCompleted>(completed => this.OnCaptureTrackingCompleted(completed));
    }
  }
}
