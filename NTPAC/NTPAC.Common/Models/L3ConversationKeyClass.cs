using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using PacketDotNet.IP;

namespace NTPAC.Common.Models
{
    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
    public class L3ConversationKeyClass
    {
        /// <summary>
        ///     Should not be accessed, only by serializator!
        /// </summary>
        public L3ConversationKeyClass() { }

        public L3ConversationKeyClass(IPAddress address1, IPAddress address2) => this.Key = GetKey(address1, address2);

        public L3ConversationKeyClass(IpPacket ipPacket) : this(ipPacket.SourceAddress, ipPacket.DestinationAddress) { }

        /// <summary>
        ///     Should not be accessed, only by serializator!
        /// </summary>
        public Byte[] Key { get; set; }

        public static Boolean Equals(L3ConversationKeyClass key1, L3ConversationKeyClass key2)
        {
            if (key1.Key.Length != key2.Key.Length)
            {
                return false;
            }

            for (var i = 0; i < key1.Key.Length; i++) //for is faster then Linq
            {
                if (key1.Key[i] != key2.Key[i])
                {
                    return false;
                }
            }

            return true;
        }


        public Boolean Equals(L3ConversationKeyClass other) => Equals(this, other);

        public override Boolean Equals(Object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is L3ConversationKeyClass key && this.Equals(key);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override Int32 GetHashCode() => this.Key != null ? this.GetHashCode(this.Key) : 0;

        public Int32 GetHashCode(Byte[] array)
        {
            unchecked
            {
                if (array == null)
                {
                    return 0;
                }

                var hash = 17;
                foreach (var element in array)
                {
                    hash = hash * 31 + element.GetHashCode();
                }

                return hash;
            }
        }

        public static Boolean operator ==(L3ConversationKeyClass key1, L3ConversationKeyClass key2) => Equals(key1, key2);

        public static Boolean operator !=(L3ConversationKeyClass key1, L3ConversationKeyClass key2) => !Equals(key1, key2);


        public override String ToString()
        {
            var delimPos = this.Key.Length / 2;
            var address1 = new IPAddress(this.Key.Take(delimPos).ToArray());
            var address2 = new IPAddress(this.Key.Skip(delimPos).ToArray());
            return $"{address1}:{address2}";
        }

        private static Byte[] GetKey(IPAddress address1, IPAddress address2)
        {
            var address1Bytes = address1.GetAddressBytes();
            var address2Bytes = address2.GetAddressBytes();

            for (var i = 0; i < address1Bytes.Length; i++)
            {
                if (address1Bytes[i] < address2Bytes[i])
                {
                    return TransformKey(address1Bytes, address2Bytes);
                }

                if (address1Bytes[i] > address2Bytes[i])
                {
                    return TransformKey(address2Bytes, address1Bytes);
                }
            }

            return TransformKey(address1Bytes, address2Bytes);
        }

        private static Byte[] TransformKey(Byte[] address1, Byte[] address2)
        {
            var key = new Byte[address1.Length + address2.Length];

            var index = 0;
            for (var i = 0; i < address1.Length; i++, index++)
            {
                key[index] = address1[i];
            }

            for (var i = 0; i < address1.Length; i++, index++)
            {
                key[index] = address2[i];
            }

            return key;
        }
    }
}
