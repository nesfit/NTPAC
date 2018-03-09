using System;
using System.Collections;
using System.Collections.Generic;

namespace NTPAC.Common.Helpers
{
  public static class LazyComparableMerger
  {
    public static IEnumerable<T> Merge<T>(IEnumerable<T> first, IEnumerable<T> second) where T: IComparable<T>
    {
      Boolean InitializeEnumeratorIfAny(IEnumerator enumerator) => enumerator.MoveNext();
      var firstEnumerator  = first.GetEnumerator();
      var secondEnumerator = second.GetEnumerator();

      var isFirstAny  = InitializeEnumeratorIfAny(firstEnumerator);
      var isSecondAny = InitializeEnumeratorIfAny(secondEnumerator);
      
      if (!isFirstAny && !isSecondAny)
      {
        yield break;
      }

      if (!isFirstAny)
      {
        do
        {
          yield return secondEnumerator.Current;
        } while (secondEnumerator.MoveNext());

        yield break;
      }

      if (!isSecondAny)
      {
        do
        {
          yield return firstEnumerator.Current;
        } while (firstEnumerator.MoveNext());

        yield break;
      }

      while (true)
      {
        IEnumerator<T> currentEnumerator, otherEnumerator;
        if (firstEnumerator.Current.CompareTo(secondEnumerator.Current) <= 0)
        {
          currentEnumerator = firstEnumerator;
          otherEnumerator   = secondEnumerator;
        }
        else
        {
          currentEnumerator = secondEnumerator;
          otherEnumerator   = firstEnumerator;
        }

        yield return currentEnumerator.Current;
        if (!currentEnumerator.MoveNext())
        {
          do
          {
            yield return otherEnumerator.Current;
          } while (otherEnumerator.MoveNext());

          yield break;
        }
      }
    }
  }
}
