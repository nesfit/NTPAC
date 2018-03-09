using System;
using System.Collections.Generic;

namespace NTPAC.ConversationTracking.Models
{
  public class L7ConversationPdusShard
  {
    public Guid L7ConversationId { get; set; }
    public UInt32 Shard { get; set; }
    public IEnumerable<L7Pdu> Pdus { get; set; }
  }
}
