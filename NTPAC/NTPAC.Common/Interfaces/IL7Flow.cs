using System;
using System.Collections.Generic;
using NTPAC.ConversationTracking.Models;

namespace NTPAC.Common.Interfaces
{
  public interface IL7Flow
  {
    Int64? FirstSeenTicks { get; }
    UInt32? FlowIdentifier { get; }
    IEnumerable<L7Pdu> L7Pdus { get; }
    Int64? LastSeenTicks { get; }
    IEnumerable<IFrame> NonDataFrames { get; }
    void AddNonDataFrame(IFrame frame);
    void AddPdu(L7Pdu pdu);
    Boolean Any();
    void SetPaired();
  }
}
