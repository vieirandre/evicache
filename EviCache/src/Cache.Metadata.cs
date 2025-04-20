using EviCache.Abstractions;
using EviCache.Models;

namespace EviCache;

public partial class Cache<TKey, TValue> : ICacheMetadata<TKey> where TKey : notnull
{
    public CacheItemMetadata GetMetadata(TKey key)
    {
        lock (_syncLock)
        {
            if (_cacheMap.TryGetValue(key, out var item))
                return item.Metadata;

            throw new KeyNotFoundException($"The key '{key}' was not found in the cache");
        }
    }

    public bool TryGetMetadata(TKey key, out CacheItemMetadata? metadata)
    {
        lock (_syncLock)
        {
            if (_cacheMap.TryGetValue(key, out var item))
            {
                metadata = item.Metadata;
                return true;
            }

            metadata = default;
            return false;
        }
    }
}
