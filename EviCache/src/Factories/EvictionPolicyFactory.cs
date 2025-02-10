using EviCache.Abstractions;
using EviCache.Enums;
using EviCache.Policies;

namespace EviCache.Factories;

public static class EvictionPolicyFactory
{
    public static IEvictionPolicy<TKey, TValue> Create<TKey, TValue>(EvictionPolicyType policyType) where TKey : notnull
    {
        return policyType switch
        {
            EvictionPolicyType.LRU => new LruEvictionPolicy<TKey, TValue>(),
            _ => throw new NotSupportedException($"The eviction policy '{policyType}' is not supported")
        };
    }
}
