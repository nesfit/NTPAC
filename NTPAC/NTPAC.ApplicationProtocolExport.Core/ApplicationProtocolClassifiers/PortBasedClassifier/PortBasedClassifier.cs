using System;
using System.IO;
using System.Reflection;
using NTPAC.ApplicationProtocolExport.Interfaces;
using NTPAC.ConversationTracking.Interfaces;
using PacketDotNet;

namespace NTPAC.ApplicationProtocolExport.Core.ApplicationProtocolClassifiers.PortBasedClassifier
{
  public class PortBasedClassifier : IApplicationProtocolClassifier
  {
    private readonly ServiceLookupTable _serviceLookupTable = new ServiceLookupTable();
    
    public PortBasedClassifier()
    {
      this.LoadServices();
    }

    private Stream GetServicesStream()
    {
      var assembly = Assembly.GetExecutingAssembly();
      var fileFullName = $"{typeof(PortBasedClassifier).Namespace}.Resources.service-names-port-numbers.csv"; 
      var servicesStream = assembly.GetManifestResourceStream(fileFullName);
      if (servicesStream == null)
      {
        throw new Exception("Failed to load services service-names-port-numbers.csv");
      }

      return servicesStream;
    }
    
    private void LoadServices()
    {
      using (var streamReader = new StreamReader(this.GetServicesStream()))
      {
        for (var line = streamReader.ReadLine(); line != null; line = streamReader.ReadLine())
        {
          var components = line.Split(',');
          if (components.Length < 4)
          {
            continue;
          }
          var name        = components[0];
          var portStr     = components[1];
          var protocolStr = components[2];

          if (String.IsNullOrWhiteSpace(portStr) || String.IsNullOrWhiteSpace(name) || String.IsNullOrWhiteSpace(protocolStr))
          {
            continue;
          }

          if (!UInt16.TryParse(portStr, out var port))
          {
            continue;
          }

          IPProtocolType protocol;
          switch (protocolStr)
          {
              case "tcp":
                protocol = IPProtocolType.TCP;
                break;
              case "udp":
                protocol = IPProtocolType.UDP;
                break;
              default:
                continue;
          }

          var serviceRecord = new ServiceRecord {Name = name, Port = port, ProtocolType = protocol};
          this._serviceLookupTable.Add(serviceRecord);
        }
      }
    }
    
    public String Classify(IL7Conversation l7Conversation)
    {
      var port = (UInt16) Math.Min(l7Conversation.DestinationEndPoint.Port, l7Conversation.SourceEndPoint.Port);
      var protocol = l7Conversation.ProtocolType;
      return this._serviceLookupTable.TryLookupByPortAndProtocol(port, protocol, out var serviceRecord) ? serviceRecord.Name : null;
    }
  }
}
