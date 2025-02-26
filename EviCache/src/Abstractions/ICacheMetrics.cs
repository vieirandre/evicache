namespace EviCache.Abstractions;

/// <summary>
/// Exposes metrics for monitoring cache performance and usage.
/// </summary>
public interface ICacheMetrics
{
    /// <summary>
    /// Gets the maximum number of items the cache can hold.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Gets the current number of items stored in the cache.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the total number of cache hits.
    /// </summary>
    long Hits { get; }

    /// <summary>
    /// Gets the total number of cache misses.
    /// </summary>
    long Misses { get; }

    /// <summary>
    /// Gets the total number of evictions performed by the cache.
    /// </summary>
    long Evictions { get; }
}
