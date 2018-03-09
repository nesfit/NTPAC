using System;
using System.Collections.Generic;
using System.Net;
using NTPAC.ConversationTracking.Interfaces.Enums;
using NTPAC.ConversationTracking.Models;
using NTPAC.Reassembling.Exceptions;
using PacketDotNet;

namespace NTPAC.Reassembling
{
  public abstract class L7ConversationTrackerBase
  {
    public static readonly Int64 SessionTimeoutTicks = TimeSpan.FromMinutes(10).Ticks;
    
    protected L7ConversationTrackerBase(IPEndPoint sourceEndPoint, IPEndPoint destinationEndPoint)
    {
      this.SourceEndPoint      = sourceEndPoint;
      this.DestinationEndPoint = destinationEndPoint;
    }

    public IPEndPoint DestinationEndPoint { get; }
    public abstract IPProtocolType ProtocolType { get; }
    public IPEndPoint SourceEndPoint { get; }

    public abstract IEnumerable<L7Conversation> Complete();
    public abstract L7Conversation ProcessFrame(Frame frame);

    protected L7Conversation CreateL7Conversation(L7Flow upFlow, L7Flow downFlow) =>
      new L7Conversation(this.SourceEndPoint, this.DestinationEndPoint, this.ProtocolType, upFlow, downFlow);

    protected FlowDirection GetFlowDirectionForFrame(Frame frame)
    {
      if (frame.IpProtocol != this.ProtocolType)
      {
        throw new ReassemblingException(frame, $"Invalid IP protocol (got {frame.IpProtocol}, expected {this.ProtocolType})");
      }

      if (this.IsUpFlowPacket(frame))
      {
        return FlowDirection.Up;
      }
      if (this.IsDownFlowPacket(frame))
      {
        return FlowDirection.Down;
      }

      if (this.IsInverseUpFlowPacket(frame))
      {
        return FlowDirection.Up;
      }
      if (this.IsInverseDownFlowPacket(frame))
      {
        return FlowDirection.Down;
      }
      
      throw new ReassemblingException(frame, $"Failed to determine frame's direction (frame from a foreign L4 conversation?)");
    }

    private Boolean IsDownFlowPacket(Frame frame) =>
      frame.SourceAddress.Equals(this.DestinationEndPoint.Address) &&
      frame.SourcePort == this.DestinationEndPoint.Port            &&
      frame.DestinationAddress.Equals(this.SourceEndPoint.Address) &&
      frame.DestinationPort == this.SourceEndPoint.Port;

    private Boolean IsUpFlowPacket(Frame frame) =>
      frame.SourceAddress.Equals(this.SourceEndPoint.Address)           &&
      frame.SourcePort == this.SourceEndPoint.Port                      &&
      frame.DestinationAddress.Equals(this.DestinationEndPoint.Address) &&
      frame.DestinationPort == this.DestinationEndPoint.Port;
    
    private Boolean IsInverseDownFlowPacket(Frame frame) =>
      frame.SourceAddress.Equals(this.DestinationEndPoint.Address) &&
      frame.SourcePort == this.SourceEndPoint.Port                 &&
      frame.DestinationAddress.Equals(this.SourceEndPoint.Address) &&
      frame.DestinationPort == this.DestinationEndPoint.Port;

    private Boolean IsInverseUpFlowPacket(Frame frame) =>
      frame.SourceAddress.Equals(this.SourceEndPoint.Address)           &&
      frame.SourcePort == this.DestinationEndPoint.Port                 &&
      frame.DestinationAddress.Equals(this.DestinationEndPoint.Address) &&
      frame.DestinationPort == this.SourceEndPoint.Port;
  }
}
