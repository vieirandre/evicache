using System.Collections.Immutable;

namespace LruCache.Abstractions;

public interface ICacheUtils<TKey, TValue> where TKey : notnull
{
    ImmutableList<TKey> GetKeysInOrder();
    ImmutableDictionary<TKey, TValue> GetSnapshot();
}
