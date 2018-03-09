using System;
using System.Collections.Generic;

namespace NTPAC.Common.Extensions
{
  public static class DictionaryExtension
  {
    public static Boolean RemoveSingleReferenceValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
      where TValue : class
    {
      foreach (var item in dictionary)
      {
        if (item.Value == value)
        {
          return dictionary.Remove(item.Key);
        }
      }

      return false;
    }

    public static Boolean TryGetAndRemoveValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, out TValue value)
    {
      if (!dictionary.TryGetValue(key, out value))
      {
        return false;
      }

      dictionary.Remove(key);
      return true;
    }
  }
}
