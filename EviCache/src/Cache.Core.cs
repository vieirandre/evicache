using EviCache.Abstractions;
using EviCache.Enums;
using EviCache.Factories;

namespace EviCache;

public partial class Cache<TKey, TValue> : ICacheOperations<TKey, TValue>, ICacheMetrics, ICacheInspection<TKey, TValue>, IDisposable where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, TValue> _cacheMap;
    private readonly ICacheHandler<TKey, TValue> _cacheHandler;
    private readonly object _syncLock = new();

    public Cache(int capacity, EvictionPolicy evictionPolicy)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));

        _capacity = capacity;
        _cacheMap = new Dictionary<TKey, TValue>(capacity);
        _cacheHandler = CacheHandlerFactory.Create<TKey, TValue>(evictionPolicy);
    }
}
