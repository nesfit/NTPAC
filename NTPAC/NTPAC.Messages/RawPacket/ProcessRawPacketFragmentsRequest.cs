using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NTPAC.Common.Interfaces;

namespace NTPAC.Messages.RawPacket
{
  [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
  public class ProcessRawPacketFragmentsRequest : IMultipleValues<RawPacket>
  {
    public static readonly ProcessRawPacketFragmentsRequest EmptyInstance = new ProcessRawPacketFragmentsRequest(new RawPacket[0]);
    public ProcessRawPacketFragmentsRequest(IEnumerable<RawPacket> fragmentRequests) => this.Values = fragmentRequests;
    public IEnumerable<RawPacket> Values { get; set; }
  }
}
