// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using System.Collections.Generic;
using NTPAC.ApplicationProtocolExport.Kaitai;

// ReSharper disable All

namespace NTPAC.ApplicationProtocolExport.PduProviders.Tests.TestKaitaiStructs
{

    /// <summary>
    /// (No support for Auth-Name + Add-Name for simplicity)
    /// </summary>
    public partial class DnsPacket : KaitaiStruct
    {
        public static DnsPacket FromFile(string fileName)
        {
            return new DnsPacket(new KaitaiStream(fileName));
        }


        public enum ClassType
        {
            InClass = 1,
            Cs = 2,
            Ch = 3,
            Hs = 4,
        }

        public enum TypeType
        {
            A = 1,
            Ns = 2,
            Md = 3,
            Mf = 4,
            Cname = 5,
            Soe = 6,
            Mb = 7,
            Mg = 8,
            Mr = 9,
            Null = 10,
            Wks = 11,
            Ptr = 12,
            Hinfo = 13,
            Minfo = 14,
            Mx = 15,
            Txt = 16,
        }
        public DnsPacket(KaitaiStream p__io, KaitaiStruct p__parent = null, DnsPacket p__root = null) : base(p__io)
        {
            this.m_parent = p__parent;
            this.m_root = p__root ?? this;
            this._read();
        }
        private void _read()
        {
            this._transactionId = this.m_io.ReadU2be();
            this._flags = new PacketFlags(this.m_io, this, this.m_root);
            this._qdcount = this.m_io.ReadU2be();
            this._ancount = this.m_io.ReadU2be();
            this._nscount = this.m_io.ReadU2be();
            this._arcount = this.m_io.ReadU2be();
            this._queries = new List<Query>((int) (this.Qdcount));
            for (var i = 0; i < this.Qdcount; i++)
            {
                this._queries.Add(new Query(this.m_io, this, this.m_root));
            }
            this._answers = new List<Answer>((int) (this.Ancount));
            for (var i = 0; i < this.Ancount; i++)
            {
                this._answers.Add(new Answer(this.m_io, this, this.m_root));
            }
        }
        public partial class PointerStruct : KaitaiStruct
        {
            public static PointerStruct FromFile(string fileName)
            {
                return new PointerStruct(new KaitaiStream(fileName));
            }

            public PointerStruct(KaitaiStream p__io, DnsPacket.Label p__parent = null, DnsPacket p__root = null) : base(p__io)
            {
                this.m_parent = p__parent;
                this.m_root = p__root;
                this.f_contents = false;
                this._read();
            }
            private void _read()
            {
                this._value = this.m_io.ReadU1();
            }
            private bool f_contents;
            private DomainName _contents;
            public DomainName Contents
            {
                get
                {
                    if (this.f_contents)
                        return this._contents;
                    KaitaiStream io = this.M_Root.M_Io;
                    long _pos = io.Pos;
                    io.Seek(this.Value);
                    this._contents = new DomainName(io, this, this.m_root);
                    io.Seek(_pos);
                    this.f_contents = true;
                    return this._contents;
                }
            }
            private byte _value;
            private DnsPacket m_root;
            private DnsPacket.Label m_parent;

            /// <summary>
            /// Read one byte, then offset to that position, read one domain-name and return
            /// </summary>
            public byte Value { get { return this._value; } }
            public DnsPacket M_Root { get { return this.m_root; } }
            public DnsPacket.Label M_Parent { get { return this.m_parent; } }
        }
        public partial class Label : KaitaiStruct
        {
            public static Label FromFile(string fileName)
            {
                return new Label(new KaitaiStream(fileName));
            }

            public Label(KaitaiStream p__io, DnsPacket.DomainName p__parent = null, DnsPacket p__root = null) : base(p__io)
            {
                this.m_parent = p__parent;
                this.m_root = p__root;
                this.f_isPointer = false;
                this._read();
            }
            private void _read()
            {
                this._length = this.m_io.ReadU1();
                if (this.IsPointer) {
                    this._pointer = new PointerStruct(this.m_io, this, this.m_root);
                }
                if (!(this.IsPointer)) {
                    this._name = System.Text.Encoding.GetEncoding("ASCII").GetString(this.m_io.ReadBytes(this.Length));
                }
            }
            private bool f_isPointer;
            private bool _isPointer;
            public bool IsPointer
            {
                get
                {
                    if (this.f_isPointer)
                        return this._isPointer;
                    this._isPointer = (bool) (this.Length == 192);
                    this.f_isPointer = true;
                    return this._isPointer;
                }
            }
            private byte _length;
            private PointerStruct _pointer;
            private string _name;
            private DnsPacket m_root;
            private DnsPacket.DomainName m_parent;

            /// <summary>
            /// RFC1035 4.1.4: If the first two bits are raised it's a pointer-offset to a previously defined name
            /// </summary>
            public byte Length { get { return this._length; } }
            public PointerStruct Pointer { get { return this._pointer; } }

            /// <summary>
            /// Otherwise its a string the length of the length value
            /// </summary>
            public string Name { get { return this._name; } }
            public DnsPacket M_Root { get { return this.m_root; } }
            public DnsPacket.DomainName M_Parent { get { return this.m_parent; } }
        }
        public partial class Query : KaitaiStruct
        {
            public static Query FromFile(string fileName)
            {
                return new Query(new KaitaiStream(fileName));
            }

            public Query(KaitaiStream p__io, DnsPacket p__parent = null, DnsPacket p__root = null) : base(p__io)
            {
                this.m_parent = p__parent;
                this.m_root = p__root;
                this._read();
            }
            private void _read()
            {
                this._name = new DomainName(this.m_io, this, this.m_root);
                this._type = ((DnsPacket.TypeType) this.m_io.ReadU2be());
                this._queryClass = ((DnsPacket.ClassType) this.m_io.ReadU2be());
            }
            private DomainName _name;
            private TypeType _type;
            private ClassType _queryClass;
            private DnsPacket m_root;
            private DnsPacket m_parent;
            public DomainName Name { get { return this._name; } }
            public TypeType Type { get { return this._type; } }
            public ClassType QueryClass { get { return this._queryClass; } }
            public DnsPacket M_Root { get { return this.m_root; } }
            public DnsPacket M_Parent { get { return this.m_parent; } }
        }
        public partial class DomainName : KaitaiStruct
        {
            public static DomainName FromFile(string fileName)
            {
                return new DomainName(new KaitaiStream(fileName));
            }

            public DomainName(KaitaiStream p__io, KaitaiStruct p__parent = null, DnsPacket p__root = null) : base(p__io)
            {
                this.m_parent = p__parent;
                this.m_root = p__root;
                this._read();
            }
            private void _read()
            {
                this._name = new List<Label>();
                {
                    var i = 0;
                    Label M_;
                    do {
                        M_ = new Label(this.m_io, this, this.m_root);
                        this._name.Add(M_);
                        i++;
                    } while (!( ((M_.Length == 0) || (M_.Length == 192)) ));
                }
            }
            private List<Label> _name;
            private DnsPacket m_root;
            private KaitaiStruct m_parent;

            /// <summary>
            /// Repeat until the length is 0 or it is a pointer (bit-hack to get around lack of OR operator)
            /// </summary>
            public List<Label> Name { get { return this._name; } }
            public DnsPacket M_Root { get { return this.m_root; } }
            public KaitaiStruct M_Parent { get { return this.m_parent; } }
        }
        public partial class Address : KaitaiStruct
        {
            public static Address FromFile(string fileName)
            {
                return new Address(new KaitaiStream(fileName));
            }

            public Address(KaitaiStream p__io, DnsPacket.Answer p__parent = null, DnsPacket p__root = null) : base(p__io)
            {
                this.m_parent = p__parent;
                this.m_root = p__root;
                this._read();
            }
            private void _read()
            {
                this._ip = new List<byte>((int) (4));
                for (var i = 0; i < 4; i++)
                {
                    this._ip.Add(this.m_io.ReadU1());
                }
            }
            private List<byte> _ip;
            private DnsPacket m_root;
            private DnsPacket.Answer m_parent;
            public List<byte> Ip { get { return this._ip; } }
            public DnsPacket M_Root { get { return this.m_root; } }
            public DnsPacket.Answer M_Parent { get { return this.m_parent; } }
        }
        public partial class Answer : KaitaiStruct
        {
            public static Answer FromFile(string fileName)
            {
                return new Answer(new KaitaiStream(fileName));
            }

            public Answer(KaitaiStream p__io, DnsPacket p__parent = null, DnsPacket p__root = null) : base(p__io)
            {
                this.m_parent = p__parent;
                this.m_root = p__root;
                this._read();
            }
            private void _read()
            {
                this._name = new DomainName(this.m_io, this, this.m_root);
                this._type = ((DnsPacket.TypeType) this.m_io.ReadU2be());
                this._answerClass = ((DnsPacket.ClassType) this.m_io.ReadU2be());
                this._ttl = this.m_io.ReadS4be();
                this._rdlength = this.m_io.ReadU2be();
                if (this.Type == DnsPacket.TypeType.Ptr) {
                    this._ptrdname = new DomainName(this.m_io, this, this.m_root);
                }
                if (this.Type == DnsPacket.TypeType.A) {
                    this._address = new Address(this.m_io, this, this.m_root);
                }
            }
            private DomainName _name;
            private TypeType _type;
            private ClassType _answerClass;
            private int _ttl;
            private ushort _rdlength;
            private DomainName _ptrdname;
            private Address _address;
            private DnsPacket m_root;
            private DnsPacket m_parent;
            public DomainName Name { get { return this._name; } }
            public TypeType Type { get { return this._type; } }
            public ClassType AnswerClass { get { return this._answerClass; } }

            /// <summary>
            /// Time to live (in seconds)
            /// </summary>
            public int Ttl { get { return this._ttl; } }

            /// <summary>
            /// Length in octets of the following payload
            /// </summary>
            public ushort Rdlength { get { return this._rdlength; } }
            public DomainName Ptrdname { get { return this._ptrdname; } }
            public Address Address { get { return this._address; } }
            public DnsPacket M_Root { get { return this.m_root; } }
            public DnsPacket M_Parent { get { return this.m_parent; } }
        }
        public partial class PacketFlags : KaitaiStruct
        {
            public static PacketFlags FromFile(string fileName)
            {
                return new PacketFlags(new KaitaiStream(fileName));
            }

            public PacketFlags(KaitaiStream p__io, DnsPacket p__parent = null, DnsPacket p__root = null) : base(p__io)
            {
                this.m_parent = p__parent;
                this.m_root = p__root;
                this.f_qr = false;
                this.f_ra = false;
                this.f_tc = false;
                this.f_rcode = false;
                this.f_opcode = false;
                this.f_aa = false;
                this.f_z = false;
                this.f_rd = false;
                this.f_cd = false;
                this.f_ad = false;
                this._read();
            }
            private void _read()
            {
                this._flag = this.m_io.ReadU2be();
            }
            private bool f_qr;
            private int _qr;
            public int Qr
            {
                get
                {
                    if (this.f_qr)
                        return this._qr;
                    this._qr = (int) (((this.Flag & 32768) >> 15));
                    this.f_qr = true;
                    return this._qr;
                }
            }
            private bool f_ra;
            private int _ra;
            public int Ra
            {
                get
                {
                    if (this.f_ra)
                        return this._ra;
                    this._ra = (int) (((this.Flag & 128) >> 7));
                    this.f_ra = true;
                    return this._ra;
                }
            }
            private bool f_tc;
            private int _tc;
            public int Tc
            {
                get
                {
                    if (this.f_tc)
                        return this._tc;
                    this._tc = (int) (((this.Flag & 512) >> 9));
                    this.f_tc = true;
                    return this._tc;
                }
            }
            private bool f_rcode;
            private int _rcode;
            public int Rcode
            {
                get
                {
                    if (this.f_rcode)
                        return this._rcode;
                    this._rcode = (int) (((this.Flag & 15) >> 0));
                    this.f_rcode = true;
                    return this._rcode;
                }
            }
            private bool f_opcode;
            private int _opcode;
            public int Opcode
            {
                get
                {
                    if (this.f_opcode)
                        return this._opcode;
                    this._opcode = (int) (((this.Flag & 30720) >> 11));
                    this.f_opcode = true;
                    return this._opcode;
                }
            }
            private bool f_aa;
            private int _aa;
            public int Aa
            {
                get
                {
                    if (this.f_aa)
                        return this._aa;
                    this._aa = (int) (((this.Flag & 1024) >> 10));
                    this.f_aa = true;
                    return this._aa;
                }
            }
            private bool f_z;
            private int _z;
            public int Z
            {
                get
                {
                    if (this.f_z)
                        return this._z;
                    this._z = (int) (((this.Flag & 64) >> 6));
                    this.f_z = true;
                    return this._z;
                }
            }
            private bool f_rd;
            private int _rd;
            public int Rd
            {
                get
                {
                    if (this.f_rd)
                        return this._rd;
                    this._rd = (int) (((this.Flag & 256) >> 8));
                    this.f_rd = true;
                    return this._rd;
                }
            }
            private bool f_cd;
            private int _cd;
            public int Cd
            {
                get
                {
                    if (this.f_cd)
                        return this._cd;
                    this._cd = (int) (((this.Flag & 16) >> 4));
                    this.f_cd = true;
                    return this._cd;
                }
            }
            private bool f_ad;
            private int _ad;
            public int Ad
            {
                get
                {
                    if (this.f_ad)
                        return this._ad;
                    this._ad = (int) (((this.Flag & 32) >> 5));
                    this.f_ad = true;
                    return this._ad;
                }
            }
            private ushort _flag;
            private DnsPacket m_root;
            private DnsPacket m_parent;
            public ushort Flag { get { return this._flag; } }
            public DnsPacket M_Root { get { return this.m_root; } }
            public DnsPacket M_Parent { get { return this.m_parent; } }
        }
        private ushort _transactionId;
        private PacketFlags _flags;
        private ushort _qdcount;
        private ushort _ancount;
        private ushort _nscount;
        private ushort _arcount;
        private List<Query> _queries;
        private List<Answer> _answers;
        private DnsPacket m_root;
        private KaitaiStruct m_parent;

        /// <summary>
        /// ID to keep track of request/responces
        /// </summary>
        public ushort TransactionId { get { return this._transactionId; } }
        public PacketFlags Flags { get { return this._flags; } }

        /// <summary>
        /// How many questions are there
        /// </summary>
        public ushort Qdcount { get { return this._qdcount; } }

        /// <summary>
        /// Number of resource records answering the question
        /// </summary>
        public ushort Ancount { get { return this._ancount; } }

        /// <summary>
        /// Number of resource records pointing toward an authority
        /// </summary>
        public ushort Nscount { get { return this._nscount; } }

        /// <summary>
        /// Number of resource records holding additional information
        /// </summary>
        public ushort Arcount { get { return this._arcount; } }
        public List<Query> Queries { get { return this._queries; } }
        public List<Answer> Answers { get { return this._answers; } }
        public DnsPacket M_Root { get { return this.m_root; } }
        public KaitaiStruct M_Parent { get { return this.m_parent; } }
    }
}
