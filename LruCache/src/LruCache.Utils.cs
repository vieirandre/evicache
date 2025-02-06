using LruCache.Abstractions;
using System.Collections.Immutable;

namespace LruCache;

public partial class LruCache<TKey, TValue> : ILruCache<TKey, TValue>, ICacheMetrics, ICacheUtils<TKey, TValue>, IDisposable where TKey : notnull
{
    public ImmutableList<TKey> GetKeysInOrder()
    {
        lock (_syncLock)
        {
            return _lruList.Select(node => node.Key)
                .ToImmutableList();
        }
    }

    public ImmutableList<KeyValuePair<TKey, TValue>> GetSnapshot()
    {
        lock (_syncLock)
        {
            return _lruList
                .Select(node => new KeyValuePair<TKey, TValue>(node.Key, node.Value))
                .ToImmutableList();
        }
    }
}
