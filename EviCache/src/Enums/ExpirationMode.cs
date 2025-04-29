namespace EviCache.Enums;

/// <summary>
/// Specifies the mode of expiration for cache items.
/// </summary>
public enum ExpirationMode
{
    /// <summary>
    /// The cache item expires at a fixed time regardless of access.
    /// </summary>
    Absolute,

    /// <summary>
    /// The cache item's expiration time is extended with each access.
    /// </summary>
    Sliding
}
