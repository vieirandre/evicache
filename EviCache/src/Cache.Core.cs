using EviCache.Abstractions;
using EviCache.Factories;
using EviCache.Options;

namespace EviCache;

public partial class Cache<TKey, TValue> : ICacheOperations<TKey, TValue>, ICacheMetrics, ICacheInspection<TKey, TValue>, IDisposable where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, TValue> _cacheMap;
    private readonly ICacheHandler<TKey, TValue> _cacheHandler;
    private readonly object _syncLock = new();

    public Cache(CacheOptions options)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));
        if (options.Capacity <= 0) throw new ArgumentOutOfRangeException(nameof(options.Capacity));

        _capacity = options.Capacity;
        _cacheMap = new Dictionary<TKey, TValue>(_capacity);
        _cacheHandler = CacheHandlerFactory.Create<TKey, TValue>(options.EvictionPolicy);
    }
}
