using EviCache.Abstractions;
using EviCache.Factories;
using EviCache.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EviCache;

/// <summary>
/// Represents an in-memory, thread-safe cache with support for multiple eviction policies.
/// </summary>
/// <typeparam name="TKey">The type of keys stored in the cache.</typeparam>
/// <typeparam name="TValue">The type of values stored in the cache.</typeparam>
public partial class Cache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity;
    private readonly object _syncLock = new();
    private readonly ILogger _logger;

    private readonly ICacheHandler<TKey> _cacheHandler;
    private readonly IEvictionCandidateSelector<TKey>? _evictionCandidateSelector;

    private readonly Dictionary<TKey, TValue> _cacheMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="Cache{TKey, TValue}"/> class with the specified options.
    /// </summary>
    /// <param name="options">The configuration options, including capacity and eviction policy.</param>
    /// <param name="logger">An optional logger for logging cache events.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the capacity is zero or negative.</exception>
    public Cache(CacheOptions options, ILogger? logger = null)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));
        if (options.Capacity <= 0) throw new ArgumentOutOfRangeException(nameof(options.Capacity));

        _capacity = options.Capacity;
        _cacheMap = new Dictionary<TKey, TValue>(_capacity);
        _cacheHandler = CacheHandlerFactory.Create<TKey>(options.EvictionPolicy);
        _evictionCandidateSelector = _cacheHandler as IEvictionCandidateSelector<TKey>;

        _logger = logger ?? NullLogger<Cache<TKey, TValue>>.Instance;
        _logger.LogInformation("Cache initialized with capacity {Capacity} and eviction policy {EvictionPolicy}", options.Capacity, options.EvictionPolicy);
    }
}
