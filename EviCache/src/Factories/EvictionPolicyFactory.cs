using EviCache.Abstractions;
using EviCache.Enums;
using EviCache.EvictionPolicies;

namespace EviCache.Factories;

public static class EvictionPolicyFactory
{
    public static IEvictionPolicy<TKey, TValue> Create<TKey, TValue>(EvictionPolicy policyType) where TKey : notnull
    {
        return policyType switch
        {
            EvictionPolicy.LRU => new LruEvictionPolicy<TKey, TValue>(),
            _ => throw new NotSupportedException($"The eviction policy '{policyType}' is not supported")
        };
    }
}
