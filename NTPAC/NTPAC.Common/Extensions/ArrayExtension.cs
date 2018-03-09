using System;
using System.Collections.Generic;
using System.Linq;

namespace NTPAC.Common.Extensions
{
  public static class ArrayExtension
  {
    public static void AddRange<T>(this T[] array, Int32 index, IEnumerable<T> toAdd)
    {
      array.AddRange(index, toAdd.GetEnumerator());
    }

    public static void AddRange<T>(this T[] array, Int32 index, IEnumerator<T> toAddEnumerator)
    {
      do
      {
        if (index >= array.Length)
        {
          throw new ArgumentOutOfRangeException();
        }

        array[index++] = toAddEnumerator.Current;
      } while (toAddEnumerator.MoveNext());
    }

    // https: //stackoverflow.com/questions/16340/how-do-i-generate-a-hashcode-from-a-byte-array-in-c
    public static Int32 GetContentHashCode(this Byte[] array)
    {
      unchecked
      {
        const Int32 p = 16777619;
        var hash = array.Aggregate((Int32) 2166136261, (current, b) => (current ^ b) * p);

        hash += hash << 13;
        hash ^= hash >> 7;
        hash += hash << 3;
        hash ^= hash >> 17;
        hash += hash << 5;
        return hash;
      }
    }
  }
}
