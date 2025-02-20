using EviCache.Abstractions;
using EviCache.Enums;
using EviCache.Handlers;

namespace EviCache.Factories;

public static class CacheHandlerFactory
{
    public static ICacheHandler<TKey, TValue> Create<TKey, TValue>(EvictionPolicy policyType) where TKey : notnull
    {
        return policyType switch
        {
            EvictionPolicy.LRU => new LruCacheHandler<TKey, TValue>(),
            EvictionPolicy.LFU => new LfuCacheHandler<TKey, TValue>(),
            _ => throw new NotSupportedException($"The eviction policy '{policyType}' is not supported")
        };
    }
}
