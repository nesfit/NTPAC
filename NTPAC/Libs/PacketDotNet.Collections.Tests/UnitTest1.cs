using System;
using System.Collections.Generic;
using Xunit;

namespace PacketDotNet.Collections.Tests
{
    public class ReassemblingCollectionTests
    {
        [Fact]
        public void ArrayCastToICollectionContainsWorks()
        {
            ICollection<C> arr = new[] {new C(1), new C(2), new C(3)};
            Assert.True(arr.Contains(new C(2)));
            Assert.False(arr.Contains(new C(4)));
        }

        [Fact]
        public void ArrayCastToICollectionCountWorks() { Assert.Equal(3, ((ICollection<String>) new[] {"x", "y", "z"}).Count); }

        [Fact]
        public void ArrayImplementsICollection() { Assert.True((object) new int[1] is ICollection<int>); }

        [Fact]
        public void ClassImplementingICollectionAddWorks()
        {
            ReassemblingCollection<string> c = new ReassemblingCollection<string>(new[] {"x", "y"});
            c.Add("z");
            Assert.Equal(3, c.Count);
            Assert.Contains("z", c);
        }

        [Fact]
        public void ClassImplementingICollectionCastToICollectionAddWorks()
        {
            ICollection<string> c = new ReassemblingCollection<string>(new[] {"x", "y"});
            c.Add("z");
            Assert.Equal(3, c.Count);
            Assert.True(c.Contains("z"));
        }

        [Fact]
        public void ClassImplementingICollectionCastToICollectionClearWorks()
        {
            ICollection<string> c = new ReassemblingCollection<string>(new[] {"x", "y"});
            c.Clear();
            Assert.Equal(0, c.Count);
        }

        [Fact]
        public void ClassImplementingICollectionCastToICollectionContainsWorks()
        {
            ICollection<string> c = new ReassemblingCollection<string>(new[] {"x", "y"});
            Assert.True(c.Contains("x"));
            Assert.False(c.Contains("z"));
        }

        [Fact]
        public void ClassImplementingICollectionCastToICollectionCountWorks()
        {
            Assert.Equal(3, ((ICollection<string>) new ReassemblingCollection<string>(new[] {"x", "y", "z"})).Count);
        }

        [Fact]
        public void ClassImplementingICollectionCastToICollectionRemoveWorks()
        {
            ICollection<string> c = new ReassemblingCollection<string>(new[] {"x", "y"});
            c.Clear();
            Assert.Equal(0, c.Count);
        }

        [Fact]
        public void ClassImplementingICollectionClearWorks()
        {
            ReassemblingCollection<string> c = new ReassemblingCollection<string>(new[] {"x", "y"});
            c.Clear();
            Assert.Empty(c);
        }

        [Fact]
        public void ClassImplementingICollectionContainsWorks()
        {
            ReassemblingCollection<string> c = new ReassemblingCollection<string>(new[] {"x", "y"});
            Assert.Contains("x", c);
            Assert.DoesNotContain("z", c);
        }

        [Fact]
        public void ClassImplementingICollectionCountWorks() { Assert.Equal(2, new ReassemblingCollection<string>(new[] {"x", "y"}).Count); }

        [Fact]
        public void ClassImplementingICollectionRemoveWorks()
        {
            ReassemblingCollection<string> c = new ReassemblingCollection<string>(new[] {"x", "y"});
            c.Clear();
            Assert.Empty(c);
        }

        [Fact]
        public void CustomClassThatShouldImplementICollectionDoesSo() { Assert.True((object) new ReassemblingCollection<string>(new string[0]) is ICollection<string>); }

        private class C
        {
            private readonly int _i;

            public C(int i) => this._i = i;

            public override bool Equals(object o) => o is C && this._i == ((C) o)._i;

            public override int GetHashCode() => this._i;
        }
    }
}
