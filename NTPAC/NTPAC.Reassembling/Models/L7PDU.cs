using System;
using System.Collections.Generic;
using System.Linq;
using NTPAC.Common.Models;
using NTPAC.Reassembling.Enums;

namespace NTPAC.Reassembling.Models
{
    using Timestamp = Int64;

    public class L7PDU
    {
        private L7PDU(FlowDirection direction) => this.Direction = direction;

        public L7PDU(Frame frame, FlowDirection direction) : this(direction) => this.Frames = new Frame[1]{ frame };

        public L7PDU(IEnumerable<Frame> frames, FlowDirection direction) : this(direction) => this.Frames = frames.ToArray();

        public readonly IEnumerable<Frame> Frames;

        public readonly FlowDirection Direction;

        public Timestamp FirstSeen => this.Frames.First().TimestampTicks;
        public Timestamp LastSeen => this.Frames.Last().TimestampTicks;

    }
}
