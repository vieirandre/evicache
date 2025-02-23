using EviCache.Abstractions;
using EviCache.Factories;
using EviCache.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EviCache;

public partial class Cache<TKey, TValue> : ICacheOperations<TKey, TValue>, ICacheMetrics, ICacheInspection<TKey, TValue>, IDisposable where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, TValue> _cacheMap;
    private readonly ICacheHandler<TKey, TValue> _cacheHandler;
    private readonly object _syncLock = new();
    private readonly ILogger _logger;

    public Cache(CacheOptions options, ILogger? logger = null)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));
        if (options.Capacity <= 0) throw new ArgumentOutOfRangeException(nameof(options.Capacity));

        _capacity = options.Capacity;
        _cacheMap = new Dictionary<TKey, TValue>(_capacity);
        _cacheHandler = CacheHandlerFactory.Create<TKey, TValue>(options.EvictionPolicy);

        _logger = logger ?? NullLogger<Cache<TKey, TValue>>.Instance;
        _logger.LogInformation("Cache initialized with capacity {Capacity} and eviction policy {EvictionPolicy}", options.Capacity, options.EvictionPolicy);
    }
}
