using System.Collections.Generic;
using System.Linq;

namespace RestApiConfigurationProvider.Extensions;

internal static class DictionaryExtensions
{
    /// <summary>
    /// Merges nested dictionaries into a single dictionary.
    /// It considers a custom equality comparison for the keys.
    /// </summary>
    public static IDictionary<TKey, TValue> Merge<TIgnoredKey, TKey, TValue>(
        this IDictionary<TIgnoredKey, IDictionary<TKey, TValue>> source, IEqualityComparer<TKey> comparer)
    {
        return source?
            .SelectMany(dict => dict.Value)
            .ToLookup(pair => pair.Key, pair => pair.Value, comparer)
            .ToDictionary(group => group.Key, group => group.First(), comparer);
    }
}
