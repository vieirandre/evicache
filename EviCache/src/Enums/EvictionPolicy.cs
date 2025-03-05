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
    LFU,
    /// <summary>
    /// First-In, First-Out (FIFO): Evicts the item that was inserted first.
    /// </summary>
    FIFO,
    /// <summary>
    /// No Eviction: New items are not accepted when the cache is full.
    /// </summary>
    NoEviction
}