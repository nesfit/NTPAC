using System;
using System.IO;
using NTPAC.ApplicationProtocolExport.Core.PduProviders;
using NTPAC.ApplicationProtocolExport.Core.PduProviders.Enums;
using NTPAC.Common.Extensions;
using NTPAC.Reassembling.UDP;
using NTPAC.Tests;
using Xunit;
using Xunit.Abstractions;

namespace NTPAC.ApplicationProtocolExport.PduProviders.Tests
{
  public class PduDataStreamTests : PduProvidersTestBase
  {
    public PduDataStreamTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Read_Mixed()
    {
      var l7Conversation = this.ReassembleSingleL7Conversation<UdpConversationTracker>(TestPcapFile.Dns1);
      var stream = new PduDataStream(l7Conversation, PduDataProviderType.Mixed);

      Int32 readBytes;
      Byte[] buff;
      
      buff = new Byte[46];
      readBytes = stream.Read(buff, 0, buff.Length);
      Assert.Equal(buff.Length, readBytes);
      Assert.Equal(644053552, buff.GetContentHashCode());
      Assert.Equal(46, stream.Position);
      
      buff = new Byte[146];
      readBytes = stream.Read(buff, 0, buff.Length);
      Assert.Equal(buff.Length, readBytes);
      Assert.Equal(935504716, buff.GetContentHashCode());
      Assert.Equal(192, stream.Position);
    }
    
    [Fact]
    public void Read_Breaked()
    {
      var l7Conversation = this.ReassembleSingleL7Conversation<UdpConversationTracker>(TestPcapFile.Dns1);
      var stream         = new PduDataStream(l7Conversation, PduDataProviderType.Breaked);

      Int32  readBytes;
      Byte[] buff;
      
      buff      = new Byte[46];
      readBytes = stream.Read(buff, 0, buff.Length);
      Assert.Equal(buff.Length, readBytes);
      Assert.Equal(644053552, buff.GetContentHashCode());
      Assert.Equal(46, stream.Position);
      
      buff      = new Byte[146];
      readBytes = stream.Read(buff, 0, buff.Length);
      Assert.Equal(0, readBytes);
      Assert.True(stream.EndOfStream);
      
      Assert.True(stream.NewMessage());
      
      readBytes = stream.Read(buff, 0, buff.Length);
      Assert.Equal(buff.Length, readBytes);
      Assert.Equal(935504716, buff.GetContentHashCode());
      Assert.Equal(192, stream.Position);
    }
    
    [Fact]
    public void SeekForward()
    {
      var l7Conversation = this.ReassembleSingleL7Conversation<UdpConversationTracker>(TestPcapFile.Dns1);
      var stream         = new PduDataStream(l7Conversation, PduDataProviderType.Mixed);

      Int32  readBytes;
      Byte[] buff;
      Int64 p;
      
      p = stream.Seek(46, SeekOrigin.Begin);
      Assert.Equal(46, p);
      
      buff      = new Byte[146];
      readBytes = stream.Read(buff, 0, buff.Length);
      
      Assert.Equal(buff.Length, readBytes);
      Assert.Equal(935504716, buff.GetContentHashCode());
      Assert.Equal(192, stream.Position);
    }
    
    [Fact]
    public void SeekBegin()
    {
      var l7Conversation = this.ReassembleSingleL7Conversation<UdpConversationTracker>(TestPcapFile.Dns1);
      var stream         = new PduDataStream(l7Conversation, PduDataProviderType.Mixed);

      Int32  readBytes;
      Byte[] buff;
      
      Assert.Equal(635203591241204270, stream.CurrentPdu.FirstSeenTicks);
      buff      = new Byte[46];
      readBytes = stream.Read(buff, 0, buff.Length);
      Assert.Equal(635203591241204270, stream.CurrentPdu.FirstSeenTicks);
      Assert.Equal(buff.Length, readBytes);
      Assert.Equal(644053552, buff.GetContentHashCode());
      Assert.Equal(46, stream.Position);

      stream.Seek(0, SeekOrigin.Begin);

      Assert.Equal(635203591241204270, stream.CurrentPdu.FirstSeenTicks);
      buff      = new Byte[46];
      readBytes = stream.Read(buff, 0, buff.Length);
      Assert.Equal(635203591241204270, stream.CurrentPdu.FirstSeenTicks);
      Assert.Equal(buff.Length, readBytes);
      Assert.Equal(644053552, buff.GetContentHashCode());
      Assert.Equal(46, stream.Position);
    }

    [Fact]
    public void NewMessage()
    {
      var l7Conversation = this.ReassembleSingleL7Conversation<UdpConversationTracker>(TestPcapFile.Dns1);
      var stream         = new PduDataStream(l7Conversation, PduDataProviderType.Mixed);
      
      Assert.False(stream.EndOfStream);
      
      Assert.Equal(0, stream.Position);
      Assert.Equal(635203591241204270, stream.CurrentPdu.FirstSeenTicks);
      Assert.True(stream.NewMessage());
      Assert.False(stream.EndOfStream);
      
      Assert.Equal(46, stream.Position);
      Assert.Equal(635203591242516580, stream.CurrentPdu.FirstSeenTicks);
      Assert.False(stream.NewMessage());
      Assert.True(stream.EndOfStream);    
    }
    
    [Fact]
    public void NewMessagePartial()
    {
      var l7Conversation = this.ReassembleSingleL7Conversation<UdpConversationTracker>(TestPcapFile.Dns1);
      var stream         = new PduDataStream(l7Conversation, PduDataProviderType.Mixed);
      
      var buff = new Byte[10];
      
      Assert.Equal(0, stream.Position);
      
      stream.Read(buff, 0, buff.Length);
      Assert.Equal(10, stream.Position);
      Assert.Equal(635203591241204270, stream.CurrentPdu.FirstSeenTicks);
      Assert.True(stream.NewMessage());
      Assert.False(stream.EndOfStream);
      
      Assert.Equal(46, stream.Position);
      Assert.Equal(635203591242516580, stream.CurrentPdu.FirstSeenTicks);
      Assert.False(stream.NewMessage());
      Assert.True(stream.EndOfStream);    
    }
  }
}
