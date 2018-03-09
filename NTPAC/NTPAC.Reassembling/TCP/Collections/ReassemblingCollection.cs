using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NTPAC.ConversationTracking.Interfaces.Enums;
using NTPAC.ConversationTracking.Models;
using NTPAC.Reassembling.Exceptions;
using NTPAC.Reassembling.TCP.Models;

namespace NTPAC.Reassembling.TCP.Collections
{
  public class ReassemblingCollection : ICollection<Frame>
  {
    private static readonly UInt32 MaxSequenceNumberBackwardJump = 100_000;

    private readonly LinkedList<EffectiveFrame> _linkedList;

    private Boolean _haveInitialSequenceNumber;
    private UInt32 _initialSequenceNumber;
    private LinkedListNode<EffectiveFrame> _previouslyInsertedEffectiveFrameNode;
    private UInt32 _seqNumOverflows;
//    private Boolean _seqNumShouldOverflow;

    public ReassemblingCollection(IEnumerable<Frame> items)
    {
      this._linkedList = new LinkedList<EffectiveFrame>(items.Select(frame => new EffectiveFrame(frame)));
      if (this._linkedList.Any())
      {
        this._previouslyInsertedEffectiveFrameNode = this._linkedList.Last;
      }
    }

    public ReassemblingCollection() => this._linkedList = new LinkedList<EffectiveFrame>();

    public Int32 Count => this._linkedList.Count;

    public Boolean IsReadOnly => false;

    private static void CheckEffectiveFrameNodeOverlaps(LinkedListNode<EffectiveFrame> effectiveFrameNode)
    {
      var relativeOffsetBegin = effectiveFrameNode.Value.RelativeOffsetBegin;
      var relativeOffsetEnd   = effectiveFrameNode.Value.RelativeOffsetEnd;

      var leftPayloadMismatch = false;
      var previousNode        = effectiveFrameNode.Previous;
      if (previousNode        != null                                &&
          relativeOffsetBegin < previousNode.Value.RelativeOffsetEnd &&
          effectiveFrameNode.Value.PayloadsOverlapMismatch(previousNode.Value))
      {
        leftPayloadMismatch = true;
      }

      var rightPayloadMismatch = false;
      var nextNode             = effectiveFrameNode.Next;
      if (nextNode          != null                              &&
          relativeOffsetEnd > nextNode.Value.RelativeOffsetBegin &&
          effectiveFrameNode.Value.PayloadsOverlapMismatch(nextNode.Value))
      {
        rightPayloadMismatch = true;
      }

      if (leftPayloadMismatch || rightPayloadMismatch)
      {
        throw new ReassemblingException(effectiveFrameNode.Value.Frame, "Overlapping sequence numbers");
      }
    }

    private void CheckIfFrameIsKeepAlive(Frame frame)
    {
      if (this._linkedList == null || !this._linkedList.Any())
      {
        return;
      }

      // KeepAlive should have a length of a zero or one byte
      if (frame.L7PayloadDataSegmentLength > 1)
      {
        return;
      }

      // KeepAlive should have *only* ACK flag set
      if (frame.TcpFlags != TcpFlags.Ack)
      {
        return;
      }

      // TODO possiblyKeepAlive (packets out of order, keep alive preceding data packet) ?
      
      var relativeSequenceNumber = this.RelativeSequenceNumberForFrame(frame);
      var previousEffectiveFrameNode = this._linkedList.Last;
      const Int32 maxRetroJump = 3; // part of a heuristic :)
      for (var i = 0; i < maxRetroJump && previousEffectiveFrameNode != null; i++, previousEffectiveFrameNode = previousEffectiveFrameNode.Previous)
      {        
        var previousEffectiveFrame = previousEffectiveFrameNode.Value;
        var nextExpectedRelativeOffsetBegin = previousEffectiveFrame.RelativeOffsetBegin + previousEffectiveFrame.PayloadLen;
        var previousFrame = previousEffectiveFrame.Frame;
        if (
          // Latch onto previous data (non-keepalive) frame
          relativeSequenceNumber == nextExpectedRelativeOffsetBegin - (UInt16) frame.L7PayloadDataSegmentLength
          // Latch onto previous keepalive frame
          || previousFrame.IsTcpKeepAlive && relativeSequenceNumber == previousEffectiveFrame.RelativeOffsetBegin - (UInt16) frame.L7PayloadDataSegmentLength)
        {
          frame.IsTcpKeepAlive = true;
          return;
        }        
      }    
    }

//    // Clear frame's payload for RST frames' "reasons" (Wireshark's notation)
//    private void ClearFrameL7PayloadDataSegmentForRst(Frame frame)
//    {
//      if (!frame.TcpFRst)
//      {
//        return;
//      }
//      
//      frame.ClearL7PayloadDataSegment();
//    }
    
//        private void CheckOrIncrementSequenceNumberOverflow(Frame frame)
//        {
//            if (frame.TcpSequenceNumber < this._initialSequenceNumber)
//            {
//                //throw new ReassemblingException(item, "_initialSequenceNumber's not bulletproof yet (multiple ISNs)");
//                this._seqNumOverflows++;
//            }
//        }

    private void CheckOrInitializeSequenceNumber(Frame frame)
    {
      if (!this._haveInitialSequenceNumber)
      {
        this._initialSequenceNumber     = frame.TcpSequenceNumber;
        this._haveInitialSequenceNumber = true;
      }
      else if (frame.TcpSequenceNumber + MaxSequenceNumberBackwardJump < this._initialSequenceNumber || frame.TcpFSyn)
      {
        this._initialSequenceNumber = frame.TcpSequenceNumber;
        this._seqNumOverflows++;
      }
      else if (frame.TcpSequenceNumber < this._initialSequenceNumber)
      {
        this._initialSequenceNumber = frame.TcpSequenceNumber;
      }
    }

    private LinkedListNode<EffectiveFrame> FindEffectiveFrameNodeToAddAfter(Frame frame)
    {
      if (this._previouslyInsertedEffectiveFrameNode == null)
      {
        Debug.Assert(!this._linkedList.Any(), "Linked list should be empty at this point");
        return null;
      }

      var lastFrameRelativeOffsetBegin = this._linkedList.Last.Value.RelativeOffsetBegin;
      var relativeOffsetBegin          = this.RelativeSequenceNumberForFrame(frame);
      // Normalize for KeepAlive frames
      if (frame.IsTcpKeepAlive)
      {
        relativeOffsetBegin++;
      }

      if (relativeOffsetBegin >= lastFrameRelativeOffsetBegin)
      {
        return this._linkedList.Last;
      }

      var currentEffectiveFrameNode = this._previouslyInsertedEffectiveFrameNode;

      // Nodes with equal begin offsets will be stored in order they were received
      // Checking forwards
      if (relativeOffsetBegin >= this._previouslyInsertedEffectiveFrameNode.Value.RelativeOffsetBegin)
      {
        var nextEffectiveFrameNode = currentEffectiveFrameNode.Next;
        while (nextEffectiveFrameNode != null && relativeOffsetBegin >= nextEffectiveFrameNode.Value.RelativeOffsetBegin)
        {
          currentEffectiveFrameNode = nextEffectiveFrameNode;
          nextEffectiveFrameNode    = currentEffectiveFrameNode.Next;
        }
      }
      // Checking backwards
      else
      {
        while (currentEffectiveFrameNode != null && relativeOffsetBegin < currentEffectiveFrameNode.Value.RelativeOffsetBegin)
        {
          currentEffectiveFrameNode = currentEffectiveFrameNode.Previous;
        }
      }

      return currentEffectiveFrameNode;
    }


    private static Boolean FrameIsRetransmission(LinkedListNode<EffectiveFrame> predecessorEffectiveFrameNode,
                                                 LinkedListNode<EffectiveFrame> efectiveFrameNode)
    {
      // TODO partial retransmits
      // TODO stronger retransmission detection (Acks and Checksums can vary for retransmits)
      // TODO compare payloads, across possibly multiple effective frames    
      var frame            = efectiveFrameNode.Value.Frame;
      var predecessorFrame = predecessorEffectiveFrameNode.Value.Frame;

      return frame.TcpSequenceNumber          == predecessorFrame.TcpSequenceNumber &&
             frame.L7PayloadDataSegmentLength            > 0                                   &&
             predecessorFrame.L7PayloadDataSegmentLength > 0;
      // && frame.TcpAcknowledgmentNumber == predecessorFrame.TcpAcknowledgmentNumber   // Disabled, not always true 
      // && frame.TcpChecksum == predecessorFrame.TcpChecksum;    // Disabled, not always true
    }

    private UInt64 RelativeSequenceNumberForFrame(Frame frame) =>
      frame.TcpSequenceNumber + UInt32.MaxValue * (UInt64) this._seqNumOverflows;

    public void Add(Frame frame)
    {
      // Ignore RST frames
      if (frame.TcpFRst)
      {
        return;
      }
      
      this.CheckOrInitializeSequenceNumber(frame);
//      this.CheckOrIncrementSequenceNumberOverflow(frame);
      this.CheckIfFrameIsKeepAlive(frame);
//      this.ClearFrameL7PayloadDataSegmentForRst(frame);
      
      LinkedListNode<EffectiveFrame> effectiveFrameNode, effectiveFrameNodePredecessor;
      effectiveFrameNode = new LinkedListNode<EffectiveFrame>(new EffectiveFrame(frame, this._seqNumOverflows));
      if ((effectiveFrameNodePredecessor = this.FindEffectiveFrameNodeToAddAfter(frame)) != null)
      {
        if (FrameIsRetransmission(effectiveFrameNodePredecessor, effectiveFrameNode))
        {
          effectiveFrameNode.Value.Frame.IsTcpRetransmission = true;
        }

        this._linkedList.AddAfter(effectiveFrameNodePredecessor, effectiveFrameNode);
      }
      else
      {
        this._linkedList.AddFirst(effectiveFrameNode);
      }

      this._previouslyInsertedEffectiveFrameNode = effectiveFrameNode;

      CheckEffectiveFrameNodeOverlaps(effectiveFrameNode);
    }

    public void Clear()
    {
      this._linkedList.Clear();
      this._previouslyInsertedEffectiveFrameNode = null;
      this._haveInitialSequenceNumber            = false;
      this._initialSequenceNumber                = 0;
      this._seqNumOverflows                      = 0;
    }

    public Boolean Contains(Frame item) => this._linkedList.Contains(new EffectiveFrame(item));

    public void CopyTo(Frame[] array, Int32 arrayIndex)
    {
      this._linkedList.CopyTo(array.Select(frame => new EffectiveFrame(frame)).ToArray(), arrayIndex);
    }

    public Boolean Remove(Frame item) => this._linkedList.Remove(new EffectiveFrame(item));

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) this._linkedList).GetEnumerator();

    public IEnumerator<Frame> GetEnumerator() => this._linkedList.Select(frame => frame.Frame).GetEnumerator();

    public IReadOnlyCollection<EffectiveFrame> EffectiveFrames => this._linkedList;
  }
}
