using EviCache.Abstractions;
using System.Collections.Immutable;

namespace EviCache;

public partial class Cache<TKey, TValue> : ICacheOperations<TKey, TValue>, ICacheMetrics, ICacheInspection<TKey, TValue>, IDisposable where TKey : notnull
{
    public ImmutableList<TKey> GetKeys()
    {
        lock (_syncLock)
        {
            return _cacheHandler.GetKeys();
        }
    }

    public ImmutableList<KeyValuePair<TKey, TValue>> GetSnapshot()
    {
        lock (_syncLock)
        {
            return _cacheHandler.GetKeys()
                .Where(key => _cacheMap.TryGetValue(key, out var value))
                .Select(key => new KeyValuePair<TKey, TValue>(key, _cacheMap[key]))
                .ToImmutableList();
        }
    }
}
