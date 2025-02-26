using EviCache.Enums;

namespace EviCache.Options;

/// <summary>
/// Configuration options for the cache.
/// </summary>
/// <param name="Capacity">The maximum number of items that the cache can store.</param>
/// <param name="EvictionPolicy">The eviction policy to use.</param>
public record CacheOptions(int Capacity, EvictionPolicy EvictionPolicy);
