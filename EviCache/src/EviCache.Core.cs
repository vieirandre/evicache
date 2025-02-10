using EviCache.Abstractions;
using EviCache.Enums;
using EviCache.Factories;

namespace EviCache;

public partial class EviCache<TKey, TValue> : ICacheOperations<TKey, TValue>, ICacheMetrics, ICacheUtils<TKey, TValue>, IDisposable where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, TValue> _cacheMap;
    private readonly IEvictionPolicy<TKey, TValue> _evictionPolicy;
    private readonly object _syncLock = new();

    public EviCache(int capacity, EvictionPolicy evictionPolicyType)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

        _capacity = capacity;
        _cacheMap = new Dictionary<TKey, TValue>(capacity);
        _evictionPolicy = EvictionPolicyFactory.Create<TKey, TValue>(evictionPolicyType);
    }
}
