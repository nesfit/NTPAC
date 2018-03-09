using System;
using System.Collections.Generic;
using System.Linq;
using NTPAC.ApplicationProtocolExport.Core.Models;
using NTPAC.ApplicationProtocolExport.Core.Serializers;
using NTPAC.Persistence.Entities.SnooperExportEntities;
using SnooperDNS;
using SnooperDNS.Models;
using SnooperHTTP.Models;

namespace NTPAC.Persistence.Entities.Mappers
{
  public static class SnooperExportCollectionMapper
  {
    public static IEnumerable<SnooperExportEntityBase> Map(SnooperExportCollection snooperExportCollection)
    {
      return snooperExportCollection.Exports.Select(exportBase => Map(snooperExportCollection, exportBase));
    }

    private static SnooperExportEntityBase Map(SnooperExportCollection snooperExportCollection, SnooperExportBase exportBase)
    {
      switch (exportBase)
      {
        case HttpMessage httpExport:
          return Map(snooperExportCollection, httpExport);
        case DnsMessage dnsExport:
          return Map(snooperExportCollection, dnsExport);
        default:
          return MapGenericExport(snooperExportCollection, exportBase);
      }      
    }
    private static HttpExportEntity Map(SnooperExportCollection exportCollection, HttpMessage httpExport)
    {
      var exportEntity = CreateAndSetupExportEntity<HttpExportEntity>(exportCollection, httpExport);
      exportEntity.Type = (SByte) httpExport.Type;
      exportEntity.Version = httpExport.Header?.HttpVersion;
      exportEntity.HeaderFields = httpExport.Header?.Fields;
      switch (httpExport.Header)
      {
        case HttpHeaderRequest headerRequest:
          exportEntity.Method = headerRequest.Method.ToString();
          exportEntity.Uri = headerRequest.RequestUri;
          break;
        case HttpHeaderResponse headerResponse:
          exportEntity.StatusCode = headerResponse.StatusCode;
          exportEntity.StatusMessage = headerResponse.StatusMessage;
          break;
      }

      if (!httpExport.ShouldIgnorePayload)
      {
        exportEntity.Payload           = httpExport.Content?.Payload;
        exportEntity.PayloadIncomplete = httpExport.Content?.PayloadIncomplete ?? true;  
      }
      
      return exportEntity;
    }

    private static DnsExportEntity Map(SnooperExportCollection exportCollection, DnsMessage dnsExport)
    {
      var exportEntity = CreateAndSetupExportEntity<DnsExportEntity>(exportCollection, dnsExport);
      exportEntity.TransactionId = dnsExport.TransactionId;
      exportEntity.Type = (SByte)dnsExport.Type;
      exportEntity.Queries = dnsExport.Queries.Select(dnsQuery => new DnsExportEntity.DnsQueryEntity
                                                                  {
                                                                    Name = dnsQuery.Name, Type = (SByte) dnsQuery.Type
                                                                  }).ToList();
      exportEntity.Answers = dnsExport.Answers.Select(dnsAnswer =>
      {
        var dnsAnswerEntity = new DnsExportEntity.DnsAnswerEntity
                              {
                                Type = (SByte)dnsAnswer.Type
                              };
        switch (dnsAnswer)
        {
          case DnsAnswerA a:
            dnsAnswerEntity.Address = a.Address;
            break;
          case DnsAnswerAAAA aaaa:
            dnsAnswerEntity.Address = aaaa.Address;
            break;
          case DnsAnswerMX mx:
            dnsAnswerEntity.Hostname = mx.Hostname;
            break;
          case DnsAnswerCNAME cname:
            dnsAnswerEntity.Hostname = cname.Hostname;
            break;
          case DnsAnswerNS ns:
            dnsAnswerEntity.Hostname = ns.Hostname;
            break;
          case DnsAnswerPTR ptr:
            dnsAnswerEntity.Hostname = ptr.Hostname;
            break;
        }
        return dnsAnswerEntity;
      }).ToList();
      return exportEntity;
    }

    private static GenericExportEntity MapGenericExport(SnooperExportCollection exportCollection, SnooperExportBase exportBase)
    {
      var genericEntity = CreateAndSetupExportEntity<GenericExportEntity>(exportCollection, exportBase);
      genericEntity.SerializedData = SnooperExportDataJsonSerializer.Serialize(exportBase);
      return genericEntity;
    }
    
    private static TSnooperExportEntity CreateAndSetupExportEntity<TSnooperExportEntity>(SnooperExportCollection exportCollection, SnooperExportBase exportBase) where TSnooperExportEntity:SnooperExportEntityBase, new() =>
      new TSnooperExportEntity
      {
        Snooper          = exportCollection.SnooperId,
        L7ConversationId = exportBase.ConversationId,
        Timestamp        = exportBase.Timestamp,
        Direction        = (SByte) exportBase.Direction,
      };
  }
}