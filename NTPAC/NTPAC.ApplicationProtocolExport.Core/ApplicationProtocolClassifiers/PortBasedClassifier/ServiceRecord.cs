using System;
using PacketDotNet;

namespace NTPAC.ApplicationProtocolExport.Core.ApplicationProtocolClassifiers.PortBasedClassifier
{
  internal class ServiceRecord
  {
    public String Name { get; set; }
    public UInt16 Port { get; set; }
    public IPProtocolType ProtocolType { get; set; }

    public override String ToString() => $"{this.Name} {this.ProtocolType.ToString()} {this.Port}";
  }
}
