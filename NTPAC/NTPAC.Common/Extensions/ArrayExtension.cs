using System;
using System.Collections.Generic;

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
    }
}
