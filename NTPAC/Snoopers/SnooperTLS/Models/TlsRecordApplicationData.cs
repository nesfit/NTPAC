using System;
using SnooperTLS.Kaitai;

namespace SnooperTLS.Models
{
  public class TlsRecordApplicationData : TlsRecordBase
  {
    public readonly Int32 DataLen; 
    
    public TlsRecordApplicationData(TlsPacket.TlsApplicationData kaitaiApplicationData)
    {
      this.DataLen = kaitaiApplicationData.Body.Length;
    }
  }
}
