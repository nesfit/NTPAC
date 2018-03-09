using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NTPAC.ConversationTracking.Interfaces.Enums;
using NTPAC.ConversationTracking.Models;
using NTPAC.Reassembling.TCP.Collections;

namespace NTPAC.Reassembling.TCP
{
  public class TcpFlowReassembler
  {
    private readonly List<L7Flow> _completedFlows = new List<L7Flow>(1);
    private readonly FlowDirection _direction;
    private readonly ReassemblingCollection _reassemblingCollection = new ReassemblingCollection();
    private L7Flow _currentFlow;
    private Frame _currentFrame;
    private L7Pdu _currentPdu;
    private Boolean _flowHalfClosed;
    
    public TcpFlowReassembler(FlowDirection direction) => this._direction = direction;

    public IEnumerable<L7Flow> CompletedFlows => this._completedFlows;

    public void Complete()
    {
      foreach (var frame in this._reassemblingCollection)
      {
        this._currentFrame = frame;
        this.CheckFlowInactivity();
        this.TrackCurrentFrame();
      }

      this.CloseCurrentFlow();
    }

    public void ProcessFrame(Frame frame) { this._reassemblingCollection.Add(frame); }

    public void RemovePairedFlows() { this._completedFlows.RemoveAll(flow => flow.Paired); }

    private void AddFrameToFlow()
    {
      Debug.Assert(this._currentFlow != null);

      if (this._currentFrame.L7PayloadDataSegmentLength == 0 || this._currentFrame.IsTcpRetransmission || this._currentFrame.IsTcpKeepAlive)
      {
        // ReSharper disable once PossibleNullReferenceException
        this._currentFlow.AddNonDataFrame(this._currentFrame);
      }
      else
      {
        if (this.CheckIfFrameFollowsMissingFrame())
        {
          this.AddPduToFlow();
        }
        
        this.AddFrameToPdu();
        if (this._currentFrame.TcpFPsh)
        {
          this.AddPduToFlow();
        }
      }
    }

    private void AddFrameToPdu()
    {
      if (this._currentPdu == null)
      {
        this._currentPdu = new L7Pdu(this._currentFrame, this._direction);
      }
      else
      {
        this._currentPdu.AddFrame(this._currentFrame);
      }
    }

    private void AddPduToFlow()
    {
      Debug.Assert(this._currentPdu != null);
      this._currentFlow.AddPdu(this._currentPdu);
      this._currentPdu = null;
    }

    private void CheckFlowInactivity()
    {
      if (this._currentFlow?.LastSeenTicks != null &&
          this._currentFrame.TimestampTicks - this._currentFlow.LastSeenTicks.Value >
          TcpConversationTracker.TcpSessionTimeoutTicks)
      {
        this.CloseCurrentFlow();
      }

      // TODO include check to maximal jump in sequence numbers (maximum number of lost bytes)   
    }

    private Boolean CheckIfFrameFollowsMissingFrame()
    {
      var lastFrameInPdu = _currentPdu?.Frames.Last();
      if (lastFrameInPdu == null)
      {
        return false;
      }
      return lastFrameInPdu.TcpSequenceNumber + lastFrameInPdu.L7PayloadDataSegmentLength != this._currentFrame.TcpSequenceNumber;
    }

    private void CloseCurrentFlow()
    {
      if (this._currentFlow == null)
      {
        return;
      }

      if (this._currentPdu != null)
      {
        this.AddPduToFlow();
      }

      this._completedFlows.Add(this._currentFlow);
      this._currentFlow = null;
    }

    private void OpenNewFlow()
    {
      this._currentFlow = new L7Flow();
      this.SetupNewFlow();
    }

    private void OpenNewFlow(UInt32 flowIdentifier)
    {
      this._currentFlow = new L7Flow(flowIdentifier);
      this.SetupNewFlow();
    }

    private void SetupNewFlow()
    {
      this._currentPdu     = null;
      this._flowHalfClosed = false;
      this.AddFrameToFlow();
    }

    private void TrackCurrentFrame()
    {
      // Proper flow begin
      if (this._currentFrame.TcpFSyn)
      {
        this.CloseCurrentFlow();
        // TODO use ISN of initializer ( this._currentFrame.TcpFAck ? this._currentFrame.TcpAcknowledgmentNumber - 1: this._currentFrame.TcpSequenceNumber) )
        this.OpenNewFlow(this._currentFrame.TcpFAck ?
                           this._currentFrame.TcpAcknowledgmentNumber :
                           this._currentFrame.TcpSequenceNumber + 1);
      }
      // Proper flow ending
      else if (this._currentFrame.TcpFFin)
      {
        if (this._currentFlow == null)
        {
          this.OpenNewFlow();
        }

        this.AddFrameToFlow();
        this._flowHalfClosed = true;
      }
      // Missing SYN from current flow
      // Second part of the condition handles the case when connection is half closed from this side and data frame was sent
      else if (this._currentFlow == null || !this._currentFrame.TcpFAck && this._flowHalfClosed)
      {
        this.CloseCurrentFlow();
        this.OpenNewFlow();
      }
      // Regular frame
      else
      {
        this.AddFrameToFlow();
      }
    }
  }
}
