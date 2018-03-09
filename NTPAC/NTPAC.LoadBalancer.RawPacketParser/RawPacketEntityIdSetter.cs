using System;
using System.Diagnostics;
using NTPAC.ConversationTracking.Models;
using NTPAC.Messages.RawPacket;

namespace NTPAC.LoadBalancer.RawPacketParser
{   
  public static class RawPacketEntityIdSetter
  {    
    public static Boolean SetEntityIdForRawPacket(RawPacket rawPacket, Int32 maxNumberOfShards)
    {
      var entityId = -1;
      
      if (!RawPacketSignatureParser.ExtractRawPacketSignature(rawPacket, out var sourceIpAddress, out var destinationIpAddress, out var sourcePort,
                                     out var destinationPort, out var ipProtocol, out var ipv4FragmentSignature))
      {
        return false;
      }

      if (ipv4FragmentSignature == null)
      {
        var l3L4ConversationKey = new L3L4ConversationKey(sourceIpAddress, destinationIpAddress, sourcePort, destinationPort, ipProtocol);
        entityId = l3L4ConversationKey.GetHashCode() % maxNumberOfShards;
      }
      else
      {
        // TODO fragments handling
        return false;
      }
      
      Debug.Assert(entityId >= 0 && entityId < maxNumberOfShards);
      rawPacket.EntityId = entityId;  
      
      return true;
    }
  }
}
