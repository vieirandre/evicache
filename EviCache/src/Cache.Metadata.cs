using EviCache.Abstractions;
using EviCache.Extensions;
using EviCache.Models;

namespace EviCache;

public sealed partial class Cache<TKey, TValue> : ICacheMetadata<TKey> where TKey : notnull
{
    public CacheItemMetadata GetMetadata(TKey key)
    {
        using var _ = _gate.Lock();

        if (TryGetItem(key, out var item))
            return item.Metadata;

        throw new KeyNotFoundException($"The key '{key}' was not found in the cache");
    }

    public bool TryGetMetadata(TKey key, out CacheItemMetadata? metadata)
    {
        using var _ = _gate.Lock();

        if (TryGetItem(key, out var item))
        {
            metadata = item.Metadata;
            return true;
        }

        metadata = null;
        return false;
    }
}
