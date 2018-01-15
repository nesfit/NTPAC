using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NTPAC.Common.Extensions;
using NTPAC.Reassembling.Enums;
using PacketDotNet.IP;

namespace NTPAC.Reassembling.Models
{
    public class L7Conversation
    {
        private static readonly L7PDU[] NoPDUsPlaceholder = new L7PDU[0];
        
        public L7Conversation(IPEndPoint sourceEndPoint, IPEndPoint destinationEndPoint, IPProtocolType protocolType, L7Flow upL7Flow, L7Flow downL7Flow, Int32 conversationSegmentNum = 0, Boolean lastConversationSegment = true)
        {
            this.SourceEndPoint = sourceEndPoint;
            this.DestinationEndPoint = destinationEndPoint;
            this.ProtocolType = protocolType;

            this.LastConversationSegment = lastConversationSegment;
            this.ConversationSegmentNum = conversationSegmentNum;

            this.PDUs = MergeUpDownFlowPDUs(upL7Flow, downL7Flow);
        }

        public readonly IPEndPoint SourceEndPoint;
        public readonly IPEndPoint DestinationEndPoint;
        public readonly IPProtocolType ProtocolType;

        public readonly Boolean LastConversationSegment;
        public readonly Int32 ConversationSegmentNum;

        public readonly IEnumerable<L7PDU> PDUs;
        public IEnumerable<L7PDU> UpFlowPDUs => this.L7PDUsInDirection(FlowDirection.Up);
        public IEnumerable<L7PDU> DownFlowPDUs => this.L7PDUsInDirection(FlowDirection.Down);

        private IEnumerable<L7PDU> L7PDUsInDirection(FlowDirection direction) => this.PDUs.Where(l7PDU => (l7PDU?.Direction ?? FlowDirection.None) == direction);
        
        private static IEnumerable<L7PDU> MergeUpDownFlowPDUs(L7Flow upFlow, L7Flow downFlow)
        {   
            if (upFlow == null && downFlow == null) { return NoPDUsPlaceholder; }
            if (upFlow == null) { return downFlow.L7PDUs.ToArray(); }
            if (downFlow == null) { return upFlow.L7PDUs.ToArray(); }

            var upFlowPDUs = upFlow.L7PDUs;
            var downFlowPDUs = downFlow.L7PDUs;
            
            var upFlowPDUsEnumerator = upFlowPDUs.GetEnumerator();
            var downFlowPDUsEnumerator = downFlowPDUs.GetEnumerator();

            var totalPDUs = upFlowPDUs.Count() + downFlowPDUs.Count();
            var mergedFlowPDUs = new L7PDU[totalPDUs];

            if (!upFlowPDUsEnumerator.MoveNext())
            {
                mergedFlowPDUs.AddRange(0, downFlowPDUsEnumerator);
            }
            else if (!downFlowPDUsEnumerator.MoveNext())
            {
                mergedFlowPDUs.AddRange(0, upFlowPDUsEnumerator);
            }
            else {
                for (var i = 0; i < totalPDUs; i++)
                {
                    IEnumerator<L7PDU> currentEnumerator, otherEnumerator;
                    if (upFlowPDUsEnumerator.Current.FirstSeen < downFlowPDUsEnumerator.Current.FirstSeen)
                    {
                        currentEnumerator = upFlowPDUsEnumerator;
                        otherEnumerator = downFlowPDUsEnumerator;
                    }
                    else
                    {
                        currentEnumerator = downFlowPDUsEnumerator;
                        otherEnumerator = upFlowPDUsEnumerator;
                    }
                    mergedFlowPDUs[i] = currentEnumerator.Current;
                    if (!currentEnumerator.MoveNext())
                    {
                        mergedFlowPDUs.AddRange(i + 1, otherEnumerator);
                        break;
                    }
                }    
            }
            return mergedFlowPDUs;
        }

        public override String ToString() => $"{this.SourceEndPoint}-{this.DestinationEndPoint} {this.ProtocolType} UpPDUs:{this.UpFlowPDUs?.Count()} DownPDUs:{this.DownFlowPDUs?.Count()}";
    }
}
