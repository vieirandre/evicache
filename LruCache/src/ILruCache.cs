using System.Collections.Immutable;

namespace LruCache;

public interface ILruCache<TKey, TValue>
{
    bool TryGet(TKey key, out TValue value);
    void Put(TKey key, TValue value);
    TValue GetOrAdd(TKey key, TValue value);
    bool Remove(TKey key);
    void Clear();
    int Count { get; }
    ImmutableList<TKey> GetKeysInOrder();
}
