using EviCache.Abstractions;
using EviCache.Enums;
using EviCache.Handlers;

namespace EviCache.Factories;

internal static class CacheHandlerFactory
{
    internal static CacheHandlerBase<TKey> Create<TKey>(EvictionPolicy policyType) where TKey : notnull
    {
        return policyType switch
        {
            EvictionPolicy.LRU => new LruCacheHandler<TKey>(),
            EvictionPolicy.LFU => new LfuCacheHandler<TKey>(),
            EvictionPolicy.FIFO => new FifoCacheHandler<TKey>(),
            EvictionPolicy.NoEviction => new NoEvictionCacheHandler<TKey>(),
            _ => throw new NotSupportedException($"'{policyType}' is not a supported policy")
        };
    }
}
