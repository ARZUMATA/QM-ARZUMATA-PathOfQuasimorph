using System;
using System.Collections.Generic;

public static class DictionaryExtensions
{

    // SomeDictionary.GetOrAdd(item.Id, SomeMethod);
    // SomeDictionary.GetOrAdd(item.Id, _ => fallbackData);
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> factory)
    {
        if (dict.TryGetValue(key, out var value))
            return value;

        value = factory(key);
        dict[key] = value;
        return value;
    }
}