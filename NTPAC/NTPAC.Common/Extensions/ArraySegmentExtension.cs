using System;

namespace NTPAC.Common.Extensions
{
  public static class ArraySegmentExtension
  {
    public static Boolean ContentsEqual<T>(this ArraySegment<T> arraySegment, ArraySegment<T> otherArraySegment, Int32 arraySegmentOffset, Int32 otherArraySegmentOffset, Int32 len)
      where T : IComparable<T>
    {
      var baseArray      = arraySegment.Array;
      var otherBaseArray = otherArraySegment.Array;
      if (baseArray == null || otherBaseArray == null)
      {
        throw new ArgumentNullException();
      }
      
      // Normalize length of a compared region to respect array segments' boundaries
      // TODO fix
      var maxCompLen = Math.Min(arraySegment.Count - arraySegmentOffset, arraySegmentOffset + len);
      var otherMaxCompLen = Math.Min(otherArraySegment.Count - otherArraySegmentOffset, otherArraySegmentOffset + len);
      len = Math.Min(maxCompLen, otherMaxCompLen);

      // Translate offsets to base arrays' offsets
      arraySegmentOffset += arraySegment.Offset;
      otherArraySegmentOffset += otherArraySegment.Offset;
      
      for (var i = 0; i < len; i++)
      {
        if (baseArray[arraySegmentOffset + i].CompareTo(otherBaseArray[otherArraySegmentOffset + i]) != 0)
        {
          return false;
        }
      }

      return true;
    }

    public static void CopyTo<T>(this ArraySegment<T> arraySegment, T[] dstArray, Int32 dstArrayOffset = 0)
    {
      if (arraySegment.Array == null)
      {
        throw new NullReferenceException();
      } 
      Array.Copy(arraySegment.Array, arraySegment.Offset, dstArray, dstArrayOffset, arraySegment.Count);
    } 
  }
}
