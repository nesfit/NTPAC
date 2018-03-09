using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using NTPAC.Common.Interfaces;
using NTPAC.ConversationTracking.Models;
using NTPAC.DTO.ConversationTracking;
using PacketDotNet;

namespace NTPAC.Reassembling.Models
{
  public class ReassembledL7Conversation : L7Conversation
  {
    private static readonly L7PduDTO[] NoPdusPlaceholder = new L7PduDTO[0];

    private Int64 _firstSeenTicks;

    public ReassembledL7Conversation(IPEndPoint sourceEndPoint,
                                     IPEndPoint destinationEndPoint,
                                     IPProtocolType protocolType,
                                     IL7Flow upL7Flow,
                                     IL7Flow downL7Flow,
                                     Int32 conversationSegmentNum = 0,
                                     Boolean lastConversationSegment = true)
    {
      this.SourceEndPoint      = sourceEndPoint;
      this.DestinationEndPoint = destinationEndPoint;
      this.ProtocolType        = protocolType;

      this.LastConversationSegment = lastConversationSegment;
      this.ConversationSegmentNum  = conversationSegmentNum;

      this.UpPdus   = upL7Flow?.L7Pdus   ?? NoPdusPlaceholder;
      this.DownPdus = downL7Flow?.L7Pdus ?? NoPdusPlaceholder;

      this._firstSeenTicks = Math.Max(upL7Flow?.FirstSeenTicks ?? 0, downL7Flow?.FirstSeenTicks ?? 0);
    }

    public override IEnumerable<IL7Pdu> DownPdus { get; }

    public override Int64 FirstSeenTicks
    {
      get => this._firstSeenTicks;
      set => this._firstSeenTicks = value;
    }

    public override IEnumerable<IL7Pdu> Pdus => this.MergeUpDownPdusYield();

    public override IEnumerable<IL7Pdu> UpPdus { get; }

    private IEnumerable<IL7Pdu> MergeUpDownPdusYield()
    {
      Boolean InitializeEnumeratorIfAny(IEnumerator enumerator) => enumerator.MoveNext();
      var upPdusEnumerator   = this.UpPdus.GetEnumerator();
      var downPdusEnumerator = this.DownPdus.GetEnumerator();

      var isUpFlowAny   = InitializeEnumeratorIfAny(upPdusEnumerator);
      var isDownFlowAny = InitializeEnumeratorIfAny(downPdusEnumerator);

      if (!isUpFlowAny && !isDownFlowAny)
      {
        yield break;
      }

      if (!isUpFlowAny)
      {
        do
        {
          yield return downPdusEnumerator.Current;
        } while (downPdusEnumerator.MoveNext());

        yield break;
      }

      if (!isDownFlowAny)
      {
        do
        {
          yield return upPdusEnumerator.Current;
        } while (upPdusEnumerator.MoveNext());

        yield break;
      }

      while (true)
      {
        IEnumerator<IL7Pdu> currentEnumerator, otherEnumerator;
        if (upPdusEnumerator.Current.FirstSeenTicks < downPdusEnumerator.Current.FirstSeenTicks)
        {
          currentEnumerator = upPdusEnumerator;
          otherEnumerator   = downPdusEnumerator;
        }
        else
        {
          currentEnumerator = downPdusEnumerator;
          otherEnumerator   = upPdusEnumerator;
        }

        yield return currentEnumerator.Current;
        if (!currentEnumerator.MoveNext())
        {
          do
          {
            yield return otherEnumerator.Current;
          } while (otherEnumerator.MoveNext());

          yield break;
        }
      }
    }
  }
}
