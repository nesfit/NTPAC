using System;
using System.Collections.Generic;
using System.Linq;
using NTPAC.ApplicationProtocolExport.Core.Exceptions;
using NTPAC.ApplicationProtocolExport.Core.Models;
using NTPAC.ApplicationProtocolExport.Core.PduProviders;

namespace SnooperDNS.Models
{
  public enum DnsMessageType
  {
    Query = 0,
    Answer = 1
  }
  
  public class DnsMessage : SnooperExportBase
  {
    private static DnsQuery[] NoQueries = Array.Empty<DnsQuery>();
    private static DnsAnswerBase[] NoAnswers = Array.Empty<DnsAnswerBase>();
    
    public UInt16 TransactionId { get; private set; }
    public DnsMessageType Type { get; private set; }
    public IReadOnlyCollection<DnsQuery> Queries { get; private set; }
    public IReadOnlyCollection<DnsAnswerBase> Answers { get; private set; }

    public DnsMessage(PduKaitaiReader reader) : base(reader)
    {
      this.Queries = NoQueries;
      this.Answers = NoAnswers;
      
      try
      {
        this.Parse(reader);
      }
      catch (KaitaiObjectionConstructionException e)
      {
        this.ParsingError = e;
      }
    }

    private void Parse(PduKaitaiReader reader)
    {
      var dnsPacket = reader.ReadKaitaiStruct<DnsPacket>();
      
      this.TransactionId = dnsPacket.TransactionId;
      this.Type          = dnsPacket.Flags.Qr == 0 ? DnsMessageType.Query : DnsMessageType.Answer;
      this.Queries       = dnsPacket.Queries.Select(q => new DnsQuery(q)).ToList();  
      this.Answers       = dnsPacket.Answers.Select(CreateDnsAnswerMessage).ToList();
    }

    private static DnsAnswerBase CreateDnsAnswerMessage(DnsPacket.Answer dnsAnswer)
    {
      var rData = dnsAnswer.Rdata;
      switch (dnsAnswer.Type) {
        case DnsPacket.RecordType.A: {
          return new DnsAnswerA((DnsPacket.ARecord) rData);
        }
        case DnsPacket.RecordType.Aaaa: {
          return new DnsAnswerAAAA((DnsPacket.AaaaRecord) rData);
        }
        case DnsPacket.RecordType.Mx: {
          return new DnsAnswerMX((DnsPacket.MxRecord) rData); 
        }
        case DnsPacket.RecordType.Cname: {
          return new DnsAnswerCNAME((DnsPacket.CnameRecord) rData);
        }
        case DnsPacket.RecordType.Ns: {
          return new DnsAnswerNS((DnsPacket.NsRecord) rData);
        }
        case DnsPacket.RecordType.Ptr: {
          return new DnsAnswerPTR((DnsPacket.PtrRecord) rData);
        }
        default: {
          return new DnsAnswerOther();
        }
      }
    }
  }
}
