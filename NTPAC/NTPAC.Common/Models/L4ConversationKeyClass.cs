﻿using System;
using PacketDotNet.IP;

namespace NTPAC.Common.Models
{
    public class L4ConversationKeyClass
    {
        private readonly UInt32 _key;
        private readonly IPProtocolType _protocol;

        public L4ConversationKeyClass() { }

        public L4ConversationKeyClass(UInt16 port1, UInt16 port2, IPProtocolType protocol)
        {
            this._key      = port1 > port2 ? (UInt32) ((port1 << 16) + port2) : (UInt32) ((port2 << 16) + port1);
            this._protocol = protocol;
        }

        internal L4ConversationKeyClass(UInt32 key, IPProtocolType protocol)
        {
            this._key      = key;
            this._protocol = protocol;
        }

        public Boolean Equals(L4ConversationKeyClass other) => this._key.Equals(other._key) && this._protocol == other._protocol;

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is L4ConversationKeyClass key && this.Equals(key);
        }


        public override Int32 GetHashCode()
        {
            unchecked
            {
                return (this._key.GetHashCode() * 397) ^ (Int32) this._protocol;
            }
        }

        public override String ToString()
        {
            var port1 = this._key         & 0xFFFF;
            var port2 = (this._key >> 16) & 0xFFFF;
            return $"{port1}:{port2}{this._protocol}";
        }

        public IPProtocolType GetProtocolType => this._protocol;
    }
}
