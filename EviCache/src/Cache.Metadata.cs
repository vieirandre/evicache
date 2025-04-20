using EviCache.Abstractions;
using EviCache.Models;

namespace EviCache;

public partial class Cache<TKey, TValue> : ICacheMetadataOperations<TKey> where TKey : notnull
{
    public CacheEntryMetadata GetMetadata(TKey key)
    {
        lock (_syncLock)
        {
            if (_cacheMap.TryGetValue(key, out var entry))
                return entry.Metadata;

            throw new KeyNotFoundException($"The key '{key}' was not found in the cache");
        }
    }

    public bool TryGetMetadata(TKey key, out CacheEntryMetadata? metadata)
    {
        lock (_syncLock)
        {
            if (_cacheMap.TryGetValue(key, out var entry))
            {
                metadata = entry.Metadata;
                return true;
            }

            metadata = default;
            return false;
        }
    }
}
