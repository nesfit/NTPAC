using System;
using System.IO;
using System.Text;
using NTPAC.ApplicationProtocolExport.Core.PduProviders.Extensions;
using Xunit;

namespace NTPAC.ApplicationProtocolExport.PduProviders.Tests
{
  public class StreamReaderExtensionTests
  {
    [Fact]
    public void AccessToStreamReaderPrivateFields()
    {
      Assert.NotNull(StreamReaderExtensions.CharPosField);
      Assert.NotNull(StreamReaderExtensions.ByteLenField);
      Assert.NotNull(StreamReaderExtensions.CharBufferField);
    }
    
    [Fact]
    public void GetPosition()
    {
      var s = CreateStreamFromString("abcd\nefgh\nijkl\n");
      var sr = new StreamReader(s, Encoding.ASCII);
      
      sr.ReadLine();
      Assert.Equal(5, sr.GetPrivatePosition());
      sr.ReadLine();
      Assert.Equal(10, sr.GetPrivatePosition());
      sr.ReadLine();
      Assert.Equal(15, sr.GetPrivatePosition());
    }
   
    
    private static Stream CreateStreamFromString(String str)
    {
      var stream = new MemoryStream();
      var writer = new StreamWriter(stream);
      writer.Write(str);
      writer.Flush();
      stream.Position = 0;
      return stream;
    } 
  }
}
