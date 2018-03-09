using System;
using System.Collections.Generic;
using PacketDotNet;

namespace NTPAC.ApplicationProtocolExport.Core.ApplicationProtocolClassifiers.PortBasedClassifier
{
  public class ServiceLookupTable
  {
    private readonly Dictionary<ValueTuple<UInt16, IPProtocolType>, ServiceRecord> _dict =
      new Dictionary<(UInt16, IPProtocolType), ServiceRecord>();

    internal Boolean TryLookupByPortAndProtocol(UInt16 port, IPProtocolType protocol, out ServiceRecord serviceRecord) =>
      this._dict.TryGetValue((port, protocol), out serviceRecord);

    internal void Add(ServiceRecord serviceRecord)
    {
      var key = (serviceRecord.Port, serviceRecord.ProtocolType);
      this._dict[key] = serviceRecord;
    }
  }
}
