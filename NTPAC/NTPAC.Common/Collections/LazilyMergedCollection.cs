using System;
using System.Collections;
using System.Collections.Generic;
using NTPAC.Common.Helpers;

namespace NTPAC.Common.Collections
{
  public class LazilyMergedCollection<T> :  IReadOnlyCollection<T> where T: IComparable<T>
  {
    private readonly IReadOnlyCollection<T> _first, _second;

    public LazilyMergedCollection(IReadOnlyCollection<T> first, IReadOnlyCollection<T> second)
    {
      if (first == null || second == null)
      {
        throw new ArgumentNullException();
      }
      
      this._first  = first;
      this._second = second;
    }

    public IEnumerator<T> GetEnumerator() => LazyComparableMerger.Merge(this._first, this._second).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public Int32 Count => this._first.Count + this._second.Count;
  }
}
