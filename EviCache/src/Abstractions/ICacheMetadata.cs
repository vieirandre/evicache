using EviCache.Models;

namespace EviCache.Abstractions;

/// <summary>
/// Provides access to cache item metadata.
/// </summary>
/// <typeparam name="TKey">The type of keys in the cache.</typeparam>
public interface ICacheMetadata<TKey> where TKey : notnull
{
    /// <summary>
    /// Gets the metadata for the specified cache item.
    /// </summary>
    /// <param name="key">The key of the cache item.</param>
    /// <returns>The metadata for the specified item.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the key is not found in the cache.</exception>
    CacheItemMetadata GetMetadata(TKey key);

    /// <summary>
    /// Attempts to get the metadata for the specified cache item.
    /// </summary>
    /// <param name="key">The key of the cache item.</param>
    /// <param name="metadata">When this method returns, contains the metadata associated with the key, if found; otherwise, null.</param>
    /// <returns>true if the metadata was found; otherwise, false.</returns>
    bool TryGetMetadata(TKey key, out CacheItemMetadata? metadata);
}
