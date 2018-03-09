using System;
using System.Collections;
using System.Collections.Generic;
using NTPAC.Common.Helpers;

namespace NTPAC.Common.Collections
{
  public class LazilyMergedEnumerable<T> :  IEnumerable<T> where T: IComparable<T>
  {
    private readonly IEnumerable<T> _first, _second;

    public LazilyMergedEnumerable(IEnumerable<T> first, IEnumerable<T> second)
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
  }
}
