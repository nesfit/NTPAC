using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NTPAC.ApplicationProtocolExport.Core.PduProviders.Enums;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Interfaces.Enums;

namespace NTPAC.ApplicationProtocolExport.Core.PduProviders
{
  public class PduDataStream : Stream
  {
    public readonly IL7Conversation CurrentConversation;
    private readonly List<TrackedL7Pdu> _trackedL7Pdus;
    
    private delegate Boolean PduMove(ref Int32 pduIndex);
    private readonly PduMove _pduMoveNextImpl;
    private readonly PduMove _pduMovePreviousImpl;

    private Int32 _currentPduIndex;
    private TrackedL7Pdu _messageStart;
    
    public Boolean EndOfStream { get; private set; }
    public Boolean EndOfPDU { get; private set; }

    private Int32 _readBytes;
    
    public PduDataStream(IL7Conversation l7Conversation, PduDataProviderType type)
    {
      this.CurrentConversation = l7Conversation;
      this._trackedL7Pdus = l7Conversation.Pdus.Select(pdu => new TrackedL7Pdu(pdu)).ToList();
      
      switch (type)
      {
        case PduDataProviderType.Mixed:
          this._pduMoveNextImpl = this.MoveNextMixed;
          this._pduMovePreviousImpl = this.MovePreviousMixed;
          break;
        case PduDataProviderType.SingleMessage:
          this._pduMoveNextImpl = this.MoveNextSingleMessage;
          this._pduMovePreviousImpl = this.MovePreviousSingleMessage;
          break;
        case PduDataProviderType.ContinueInterlay:
          this._pduMoveNextImpl = this.MoveNextContinueInterlay;
          this._pduMovePreviousImpl = this.MovePreviousContinueInterlay;
          break;
        case PduDataProviderType.Breaked:
          this._pduMoveNextImpl = this.MoveNextBreaked;
          this._pduMovePreviousImpl = this.MovePreviousBreaked;
          break;
        default:
          throw new ArgumentOutOfRangeException($"Invalid PduDataProviderType ${type.ToString()}");
      }
      
      this.Reset();
    }
    
    private TrackedL7Pdu TrackedL7PduAtIndex(Int32 index) => this._trackedL7Pdus[index];
    private TrackedL7Pdu CurrentTrackedL7Pdu => this.TrackedL7PduAtIndex(this._currentPduIndex);
    public IL7Pdu CurrentPdu => this._trackedL7Pdus.Any() ? this.CurrentTrackedL7Pdu.Pdu : null;


    #region Stream methods
    public override Boolean CanRead => true;
    public override Boolean CanSeek => true;
    public override Boolean CanWrite => false;
    
    public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
    {
      if (!this._trackedL7Pdus.Any())
      {
        this.EndOfStream = true;
        return 0;
      }
  
      var readBytes = 0;
      do
      {
        var bytesToReadFromPdu = Math.Min(count, this.CurrentTrackedL7Pdu.RemainingBytesToRead);
        Array.Copy(this.CurrentTrackedL7Pdu.Bytes, this.CurrentTrackedL7Pdu.Offset, buffer, offset + readBytes, bytesToReadFromPdu);
        readBytes += bytesToReadFromPdu;
        count -= bytesToReadFromPdu;
        this.CurrentTrackedL7Pdu.Offset += bytesToReadFromPdu;
        
        if (this.CurrentTrackedL7Pdu.RemainingBytesToRead == 0)
        {
          this.EndOfPDU = true;
        }

        // There are still data left to read, but we have read entire current pdu
        if (count > 0 && this.EndOfPDU)
        {
          this.EndOfStream = !this.MoveNext();
        }
      } while (!this.EndOfStream && count > 0);

      return readBytes;
    }

    public override Int64 Seek(Int64 offset, SeekOrigin origin)
    {
      var seeked = 0;
      switch (origin)
      {
        case SeekOrigin.Begin:
          if (offset <= 0)
          {
            this.Reset();
            return 0;
          }

          return this.Seek(offset - this.Position, SeekOrigin.Current);
        case SeekOrigin.Current:
          if (offset < 0)
          {
            this.EndOfStream = false;
            while (-offset > seeked)
            {
              if (-offset > this.CurrentTrackedL7Pdu.Offset + seeked)
              {
                seeked += this.CurrentTrackedL7Pdu.Offset == 0 && this._currentPduIndex != 0 ? this.CurrentTrackedL7Pdu.Length : this.CurrentTrackedL7Pdu.Offset;
                if (!this.MovePrevious())
                {
                  return this.Position;
                }
              }
              else
              {
                this.CurrentTrackedL7Pdu.Offset = this.CurrentTrackedL7Pdu.Offset - (Int32) (-offset - seeked);
                return this.Position;
              }
            }
          }
          else
          {
            while (offset > seeked)
            {
              if (offset > this.CurrentTrackedL7Pdu.RemainingBytesToRead + seeked)
              {
                seeked += this.CurrentTrackedL7Pdu.RemainingBytesToRead;
                if (!this.MoveNext())
                {
                  this.EndOfStream = true;
                  return this.Position;
                }
              }
              else
              {
                this.CurrentTrackedL7Pdu.Offset = this.CurrentTrackedL7Pdu.Offset + (Int32) (offset - seeked);
                return this.Position;
              }
            }
          }

          return this.Position;
        case SeekOrigin.End:
          throw new NotSupportedException("Seeking from the end of stream is not supported.");
        default:
          throw new ArgumentOutOfRangeException($"Not supported origin type ({origin}).");
      }
    }

    public override Int64 Position {  
      get => this._readBytes + (this._trackedL7Pdus.Any() ? this.CurrentTrackedL7Pdu.Offset : 0);
      set => this.Seek(value, SeekOrigin.Begin);
    }

    public override void Write(Byte[] buffer, Int32 offset, Int32 count) { throw new NotSupportedException(); }
    public override void Flush() { throw new NotSupportedException(); }
    
    public override void SetLength(Int64 value) { throw new NotSupportedException(); }
    public override Int64 Length => throw new NotSupportedException();
    #endregion

    private Boolean Reset()
    {
      this._readBytes = 0;
      this._currentPduIndex = 0;

      if (!this._trackedL7Pdus.Any())
      {
        this.EndOfStream = !this._trackedL7Pdus.Any();
        return false;
      }

      this._messageStart = this._trackedL7Pdus.First();
      this.ResetOffsets();
      return !this.EndOfStream;
    }

    private void ResetOffsets()
    {
      var pduIndex = this._currentPduIndex;
      do
      {
        var trackedL7Pdu = this._trackedL7Pdus[pduIndex];
        // (TODO possibly unnecessary and buggy)
        if (trackedL7Pdu.Offset == 0)
        {
          break;
        }
        trackedL7Pdu.SetUnread();
      } while (this._pduMoveNextImpl(ref pduIndex));
    }
    
    public Boolean NewMessage()
    {
      var moved = this.MoveNext(this.MoveNextMixed);
      if (moved)
      {
        this._messageStart = this.TrackedL7PduAtIndex(this._currentPduIndex);
      }
      this.EndOfStream = !moved;
      return moved;
    }

    private Boolean MoveNext() => this.MoveNext(this._pduMoveNextImpl);
    private Boolean MoveNext(PduMove pduMoveNextImpl)
    {
      var readPduBytes = this.CurrentTrackedL7Pdu.Length;
      this.CurrentTrackedL7Pdu.SetRead();

      if (!pduMoveNextImpl(ref this._currentPduIndex))
      {
        return false;
      }
      
      this.CurrentTrackedL7Pdu.SetUnread();
      this._readBytes += readPduBytes;
      return true;
    }

    private Boolean MovePrevious() => this.MovePrevious(this._pduMovePreviousImpl);
    private Boolean MovePrevious(PduMove pduMovePreviousImpl)
    {
      if (this._currentPduIndex <= 0)
      {
        return false;
      }
      
      this.CurrentTrackedL7Pdu.SetUnread();
      if (!pduMovePreviousImpl(ref this._currentPduIndex))
      {
        return false;
      }

      this._readBytes -= this.CurrentTrackedL7Pdu.Length;
      return true;
    }

    #region PduMovementImplementations
    private Boolean MoveNextMixed(ref Int32 pduIndex)
    {
      Debug.Assert(pduIndex >= 0);
      if (pduIndex >= this._trackedL7Pdus.Count - 1)
      {
        return false;
      }
      pduIndex++;
      return true;
    }
    
    private Boolean MovePreviousMixed(ref Int32 pduIndex)
    {
      Debug.Assert(pduIndex >= 0);
      if (pduIndex == 0)
      {
        return false;
      }
      pduIndex--;
      return true;
    }

    private Boolean MoveNextContinueInterlay(ref Int32 pduIndex)
    {
      var messageDirection = this._messageStart.Direction;
      var originalPduIndex = pduIndex;
      while(pduIndex != this._trackedL7Pdus.Count - 1)
      {
        pduIndex++;
        if (messageDirection == this.CurrentTrackedL7Pdu.Direction)
        {
          return true;
        }
      }
      pduIndex = originalPduIndex;
      return false;
    }
    
    private Boolean MovePreviousContinueInterlay(ref Int32 pduIndex)
    {
      var messageDirection = this._messageStart.Direction;
      while(pduIndex > 0)
      {
        pduIndex--;
        if (messageDirection == this.CurrentTrackedL7Pdu.Direction)
        {
          return true;
        }
      }
      return false;
    }

    private Boolean MoveNextBreaked(ref Int32 pduIndex)
    {
      var messageDirection = this._messageStart.Direction;
      while(pduIndex != this._trackedL7Pdus.Count - 1)
      {
        pduIndex++;
        if (messageDirection == this.CurrentTrackedL7Pdu.Direction)
        {
          return true;
        }
        pduIndex--;
        return false;
      }
      return false;
    }
    
    private Boolean MovePreviousBreaked(ref Int32 pduIndex)
    {
      if(pduIndex <= 0)
      {
        return false;
      }
      
      pduIndex--;
      if (this._messageStart.Direction == this.CurrentTrackedL7Pdu.Direction)
      {
        return true;
      }
      pduIndex--;
      return false;
    }
    
    private Boolean MoveNextSingleMessage(ref Int32 pduIndex) => false;
    
    private Boolean MovePreviousSingleMessage(ref Int32 pduIndex) => false;
    #endregion
    
    private class TrackedL7Pdu
    {
      public readonly IL7Pdu Pdu;
      public FlowDirection Direction => this.Pdu.Direction;
      public Byte[] Bytes => this.Pdu.Payload;
      public Int32 Length => this.Bytes.Length;

      public Int32 Offset { get;  set; }
      
      public Int32 RemainingBytesToRead => this.Length - this.Offset;
      
      public TrackedL7Pdu(IL7Pdu l7Pdu) => this.Pdu = l7Pdu;
      
      public void SetRead() { this.Offset = this.Length; }
      
      public void SetUnread() { this.Offset = 0; }
    }
  }
}
