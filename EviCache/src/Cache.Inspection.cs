using EviCache.Abstractions;
using System.Collections.Immutable;

namespace EviCache;

public partial class Cache<TKey, TValue> : ICacheInspection<TKey, TValue>  where TKey : notnull
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
                .Where(key => TryGetItem(key, out _))
                .Select(key => new KeyValuePair<TKey, TValue>(key, _cacheMap[key].Value))
                .ToImmutableList();
        }
    }
}
