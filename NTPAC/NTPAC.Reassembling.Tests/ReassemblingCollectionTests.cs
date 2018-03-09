using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using NTPAC.ConversationTracking.Models;
using NTPAC.Reassembling.TCP.Collections;
using NTPAC.Tests;
using Xunit;
using Xunit.Abstractions;

namespace NTPAC.Reassembling.Tests
{
  public class ReassemblingCollectionTests : ReassemblingTestBase
  {
    private readonly Frame _frame1;
    private readonly Frame _frame2;
    private readonly Frame _frame3;

    public ReassemblingCollectionTests(ITestOutputHelper output) : base(output)
    {
      this._frame1 = this.CreateFrame();
      this._frame2 = this.CreateFrame();
      this._frame3 = this.CreateFrame();
    }

    private Frame CreateFrame()
    {
      var data = new Byte[0];
      var frame = new Frame(0, data);
      frame.SetupL7PayloadDataSegment(0, 0);
      return frame;
    }

    [Fact]
    public void ClassImplementingICollectionCastToICollectionClearWorks()
    {
      var c = new ReassemblingCollection(new[] {this._frame1, this._frame2});
      c.Clear();
      Assert.Empty(c);
    }

    [Fact]
    public void ClassImplementingICollectionCastToICollectionContainsWorks()
    {
      var c = new ReassemblingCollection(new[] {this._frame1, this._frame2});
      Assert.Contains(this._frame1, c);
      Assert.DoesNotContain(this._frame3, c);
    }

    [Fact]
    public void ClassImplementingICollectionCastToICollectionCountWorks()
    {
      Assert.Throws<InvalidCastException>(
        // ReSharper disable once SuspiciousTypeConversion.Global
        () => ((ICollection<Object>) new ReassemblingCollection(new[]
                                                                {
                                                                  this._frame1,
                                                                  this._frame2,
                                                                  this._frame3
                                                                })).Count);
    }

    [Fact]
    public void ClassImplementingICollectionCastToICollectionRemoveWorks()
    {
      var c = new ReassemblingCollection(new[] {this._frame1, this._frame2});
      c.Clear();
      Assert.Empty(c);
    }

    [Fact]
    public void ClassImplementingICollectionClearWorks()
    {
      var c = new ReassemblingCollection(new[] {this._frame1, this._frame2});
      c.Clear();
      Assert.Empty(c);
    }

    [Fact]
    public void ClassImplementingICollectionContainsWorks()
    {
      var c = new ReassemblingCollection(new[] {this._frame1, this._frame2});
      Assert.Contains(this._frame1, c);
      Assert.DoesNotContain(this._frame3, c);
    }

    [Fact]
    public void ClassImplementingICollectionCountWorks()
    {
      Assert.Equal(2, new ReassemblingCollection(new[] {this._frame1, this._frame2}).Count);
    }

    [Fact]
    public void ClassImplementingICollectionRemoveWorks()
    {
      var c = new ReassemblingCollection(new[] {this._frame1, this._frame2});
      c.Clear();
      Assert.Empty(c);
    }

    [Fact]
    public void CustomClassThatShouldImplementICollectionDoesSo()
    {
      // ReSharper disable once SuspiciousTypeConversion.Global
      Assert.False((Object) new ReassemblingCollection(new[] {this._frame1, this._frame2}) is ICollection<Object>);
    }

    [Fact]
    public void TcpKeepAliveTest1()
    {
      var c = new ReassemblingCollection();
      this.AddFramesFromPcap(c, TestPcapFile.IsaHttpKeepAlive1Dst1, IPAddress.Parse("147.229.176.17"));
      Assert.Equal(19, c.Count);
    }
    
    [Fact]
    public void TcpKeepAliveTest2()
    {
      var c = new ReassemblingCollection();
      this.AddFramesFromPcap(c, TestPcapFile.TcpKeepAlive, IPAddress.Parse("10.10.10.107"));
      Assert.Equal(383, c.Count);
    }
    
    [Fact]
    public void TcpKeepAliveTest3()
    {
      var c = new ReassemblingCollection();
      this.AddFramesFromPcap(c, TestPcapFile.Sec6NetFTCP49795, IPAddress.Parse("10.10.10.229"));
      Assert.Equal(21, c.Count);
    }

    [Fact]
    public void TcpRetransmission()
    {
      var c = new ReassemblingCollection();
      this.AddFramesFromPcap(c, TestPcapFile.IsaHttpRetransmission, IPAddress.Parse("147.229.176.17"));
      Assert.Equal(6, c.Count);
    }

    [Fact]
    public void TcpReusedPortAndKeepalives()
    {
      var c = new ReassemblingCollection();
      this.AddFramesFromPcap(c, TestPcapFile.IsaHttpReusedPortsAndKeepalives, IPAddress.Parse("147.229.176.17"));
      Assert.Equal(52, c.Count);
      this.AssertEnumerableSorted(c.EffectiveFrames, ef => ef.RelativeOffsetBegin);
    }

    [Fact]
    public void TcpRstWithReason()
    {
      var c = new ReassemblingCollection();
      this.AddFramesFromPcap(c, TestPcapFile.TcpWithRstReason, IPAddress.Parse("87.106.1.89"));
      Assert.Equal(9, c.Count);
      this.AssertEnumerableSorted(c.EffectiveFrames, ef => ef.RelativeOffsetBegin);
    }

    // Partial retransmit
    [Fact]
    public void TcpOutOfOrder()
    {
      var c = new ReassemblingCollection();
      this.AddFramesFromPcap(c, TestPcapFile.TcpOutOfOrder, IPAddress.Parse("54.77.170.180"));
      Assert.Equal(17, c.Count);
    }

    private void AddFramesFromPcap(ReassemblingCollection c, String pcapFileName, IPAddress sourceIPAddress)
    {
      this.AddFramesFromPcap(c, pcapFileName, f => f.SourceAddress.Equals(sourceIPAddress));
    }

    private void AddFramesFromPcap(ReassemblingCollection c, String pcapFileName, Func<Frame, Boolean> framePredicate = null)
    {
      var frames = this.GetFramesFromPcapWithoutDefragmentation(pcapFileName);
      if (framePredicate != null)
      {
        frames = frames.Where(framePredicate);
      }

      foreach (var frame in frames)
      {
        c.Add(frame);
      }
    }

    private void AssertEnumerableSorted<T, TKey>(IEnumerable<T> enumerable, Func<T, TKey> dissectorFunc) where TKey : IComparable
    {
      var previousKey = default(TKey);
      var i           = 0;
      foreach (var item in enumerable)
      {
        var key = dissectorFunc(item);
        if (i > 0)
        {
          Debug.Assert(previousKey != null, nameof(previousKey) + " != null");
          if (previousKey.CompareTo(key) > 0)
          {
            // TODO NotSortedException
            throw new Exception("Enumerable is not sorted");
          }
        }

        previousKey = key;
        i++;
      }
    }
  }
}
