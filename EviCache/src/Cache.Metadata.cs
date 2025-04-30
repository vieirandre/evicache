using EviCache.Abstractions;
using EviCache.Models;

namespace EviCache;

public sealed partial class Cache<TKey, TValue> : ICacheMetadata<TKey> where TKey : notnull
{
    public CacheItemMetadata GetMetadata(TKey key) => WithLock(() =>
    {
        if (TryGetItem(key, out var item))
            return item.Metadata;

        throw new KeyNotFoundException($"The key '{key}' was not found in the cache");
    });

    public bool TryGetMetadata(TKey key, out CacheItemMetadata? metadata)
    {
        CacheItemMetadata? tmp = null;

        bool found = WithLock(() =>
        {
            if (TryGetItem(key, out var item))
            {
                tmp = item.Metadata;
                return true;
            }

            return false;
        });

        metadata = tmp;
        return found;
    }
}
