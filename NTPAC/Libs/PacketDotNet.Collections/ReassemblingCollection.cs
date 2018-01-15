using System;
using System.Collections;
using System.Collections.Generic;

namespace PacketDotNet.Collections
{
    public class ReassemblingCollection<T>:ICollection<T>
    {
        private readonly LinkedList<T> _linkedList;

        public ReassemblingCollection(IEnumerable<T> items) => this._linkedList =  new LinkedList<T>(items);
        public ReassemblingCollection() => this._linkedList =  new LinkedList<T>();

        public IEnumerator<T> GetEnumerator() => this._linkedList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) this._linkedList).GetEnumerator();

        public void Add(T item) { this._linkedList.AddLast(item); }

        public void Clear() { this._linkedList.Clear(); }

        public Boolean Contains(T item) => this._linkedList.Contains(item);

        public void CopyTo(T[] array, Int32 arrayIndex) { this._linkedList.CopyTo(array, arrayIndex); }

        public Boolean Remove(T item) => this._linkedList.Remove(item);

        public Int32 Count => this._linkedList.Count;

        public Boolean IsReadOnly => false;
    }
}
