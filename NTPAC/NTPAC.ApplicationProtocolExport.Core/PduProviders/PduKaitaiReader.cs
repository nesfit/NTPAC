using System;
using NTPAC.ApplicationProtocolExport.Core.Exceptions;
using NTPAC.ApplicationProtocolExport.Interfaces;
using NTPAC.ApplicationProtocolExport.Kaitai;
using NTPAC.ConversationTracking.Interfaces;

namespace NTPAC.ApplicationProtocolExport.Core.PduProviders
{
    public class PduKaitaiReader : KaitaiStream, IPduReader
    {
        private readonly PduDataStream _basePduStream;

        public PduKaitaiReader(PduDataStream basePduStream) : base(basePduStream)
        {
          this._basePduStream = basePduStream;
          this.CurrentPdu = basePduStream.CurrentPdu;
        } 
        public IL7Conversation CurrentConversation => this._basePduStream.CurrentConversation;

        public IL7Pdu CurrentPdu { get ; private set; }

        public Boolean EndOfStream => this._basePduStream.EndOfStream;

        /// <summary>
        ///     Generic form of a 'new T(this, null, null)'
        ///     null, null to match full T (Kaitai object) constructor signature
        /// </summary>
        public T ReadKaitaiStruct<T>() where T : KaitaiStruct
        {
          try
          {
            return ObjectActivatorCreator<T>.ObjectActivator(this, null, null);
          }
          catch (Exception e)
          {
            throw new KaitaiObjectionConstructionException($"Failed to construct Kaitai object of type {typeof(T)}", e);
          }
        } 

        public Boolean NewMessage()
        {
          var newMessage = this._basePduStream.NewMessage();
          this.CurrentPdu = this._basePduStream.CurrentPdu;
          return newMessage;
        }
    }
}
