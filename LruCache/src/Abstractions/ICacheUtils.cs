using System.Collections.Immutable;

namespace LruCache.Abstractions;

public interface ICacheUtils<TKey, TValue>
{
    ImmutableList<TKey> GetKeysInOrder();
    ImmutableDictionary<TKey, TValue> GetSnapshot();
}
