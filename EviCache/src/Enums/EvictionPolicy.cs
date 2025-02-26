namespace EviCache.Enums;

/// <summary>
/// Specifies the eviction policy used by the cache.
/// </summary>
public enum EvictionPolicy
{
    /// <summary>
    /// Least Recently Used (LRU): Evicts the item that has not been accessed for the longest period.
    /// </summary>
    LRU,
    /// <summary>
    /// Least Frequently Used (LFU): Evicts the item with the lowest access frequency.
    /// </summary>
    LFU
}