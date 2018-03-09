using System;
using System.IO;
using System.Reflection;

namespace NTPAC.ApplicationProtocolExport.Core.PduProviders.Extensions
{
  // https://stackoverflow.com/a/22975649
  public static class StreamReaderExtensions
  {
    public static readonly FieldInfo CharPosField = typeof(StreamReader).GetField("_charPos", BindingFlags.NonPublic | BindingFlags.Instance       | BindingFlags.DeclaredOnly);
    public static readonly FieldInfo ByteLenField = typeof(StreamReader).GetField("_byteLen", BindingFlags.NonPublic | BindingFlags.Instance       | BindingFlags.DeclaredOnly);
    public static readonly FieldInfo CharBufferField = typeof(StreamReader).GetField("_charBuffer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

    public static Int64 GetPrivatePosition(this StreamReader reader)
    {
      //shift position back from BaseStream.Position by the number of bytes read
      //into internal buffer.
      var byteLen  = (Int32)ByteLenField.GetValue(reader);
      var position = reader.BaseStream.Position - byteLen;

      //if we have consumed chars from the buffer we need to calculate how many
      //bytes they represent in the current encoding and add that to the position.
      var charPos = (Int32)CharPosField.GetValue(reader);
      if (charPos > 0)
      {
        var charBuffer    = (Char[])CharBufferField.GetValue(reader);
        var encoding      = reader.CurrentEncoding;
        var bytesConsumed = encoding.GetBytes(charBuffer, 0, charPos).Length;
        position += bytesConsumed;
      }

      return position;
    }
    
//    public static void SetPosition(this StreamReader reader, Int64 position)
//    {
//      reader.DiscardBufferedData();
//      reader.BaseStream.Seek(position, SeekOrigin.Begin);
//    }
  }
}
