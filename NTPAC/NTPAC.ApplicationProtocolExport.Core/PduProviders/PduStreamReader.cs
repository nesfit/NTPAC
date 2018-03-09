using System;
using System.IO;
using System.Text;
using NTPAC.ApplicationProtocolExport.Core.PduProviders.Extensions;
using NTPAC.ApplicationProtocolExport.Interfaces;
using NTPAC.ConversationTracking.Interfaces;

namespace NTPAC.ApplicationProtocolExport.Core.PduProviders
{
  public class PduStreamReader : StreamReader, IPduReader
  {
    private readonly PduDataStream _basePduStream;

    public PduStreamReader(PduDataStream basePduStream) : this(basePduStream, Encoding.ASCII)
    {
    }
    
    public PduStreamReader(PduDataStream basePduStream, Encoding encoding) : base(basePduStream, encoding)
    {
      this._basePduStream = basePduStream;
      this.CurrentPdu = basePduStream.CurrentPdu;
    }

    public new Boolean EndOfStream => this._basePduStream.EndOfStream && base.EndOfStream;

    public IL7Pdu CurrentPdu { get; private set; }
    public IL7Conversation CurrentConversation => this._basePduStream.CurrentConversation;

    public Int32 Read(Byte[] buffer, Int32 index, Int32 count)
    {
      this.ResetBaseStreamToCurrentPosition();
      return this._basePduStream.Read(buffer, index, count);
    }
    
    public Boolean NewMessage()
    {
      this.ResetBaseStreamToCurrentPosition();
      var newMessage = this._basePduStream.NewMessage();
      this.CurrentPdu = this._basePduStream.CurrentPdu;
      return newMessage;
    }

    // Used to counter effect the StreamReader's buffering of an underlying stream  
    private void ResetBaseStreamToCurrentPosition()
    {
      this._basePduStream.Seek(this.GetPrivatePosition(), SeekOrigin.Begin);
      this.DiscardBufferedData();
    }
  }
}
