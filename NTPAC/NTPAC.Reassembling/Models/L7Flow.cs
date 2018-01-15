using System;
using System.Collections.Generic;
using System.Linq;
using NTPAC.Reassembling.Models;

namespace NTPAC.Reassembling.Models
{
    using Timestamp = Int64;

    public class L7Flow
    {   
        private readonly List<L7PDU> _pdus = new List<L7PDU>();
        
        public Int64? FirstSeen { get; protected set; }
        public Int64? LastSeen { get; protected set; }
        public IEnumerable<L7PDU> L7PDUs => this._pdus;

        public void AddPdu(L7PDU pdu)
        {
            if (this.FirstSeen == null || this.FirstSeen > pdu.FirstSeen) { this.FirstSeen = pdu.FirstSeen; }
            if (this.LastSeen == null || this.LastSeen < pdu.LastSeen) { this.LastSeen = pdu.LastSeen; }
            this._pdus.Add(pdu);
        }

        public Boolean Any() => this._pdus.Any();
    }
}
