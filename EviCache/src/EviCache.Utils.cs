using EviCache.Abstractions;
using System.Collections.Immutable;

namespace EviCache;

public partial class EviCache<TKey, TValue> : ICacheOperations<TKey, TValue>, ICacheMetrics, ICacheUtils<TKey, TValue>, IDisposable where TKey : notnull
{
    public ImmutableList<TKey> GetKeysInOrder()
    {
        lock (_syncLock)
        {
            return _evictionPolicy.InternalCollection;
        }
    }

    public ImmutableList<KeyValuePair<TKey, TValue>> GetSnapshot()
    {
        lock (_syncLock)
        {
            return _evictionPolicy.InternalCollection
                .Where(key => _cacheMap.TryGetValue(key, out var value))
                .Select(key => new KeyValuePair<TKey, TValue>(key, _cacheMap[key]))
                .ToImmutableList();
        }
    }
}
