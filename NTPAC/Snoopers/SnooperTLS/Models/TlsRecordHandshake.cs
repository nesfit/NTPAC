using SnooperTLS.Kaitai;

namespace SnooperTLS.Models
{
  public class TlsRecordHandshake : TlsRecordBase
  {
    public readonly TlsPacket.TlsHandshakeType Type; 
    
    public TlsRecordHandshake(TlsPacket.TlsHandshake kaitaiTlsHandshake) { this.Type = kaitaiTlsHandshake.MsgType; }
  }
}
