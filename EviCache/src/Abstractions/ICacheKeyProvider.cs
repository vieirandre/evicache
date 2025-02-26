using System.Collections.Immutable;

namespace EviCache.Abstractions;

/// <summary>
/// Provides a method for retrieving all keys stored in the cache.
/// </summary>
/// <typeparam name="TKey">The type of keys in the cache.</typeparam>
public interface ICacheKeyProvider<TKey> where TKey : notnull
{
    /// <summary>
    /// Retrieves an immutable list of all keys in the cache.
    /// </summary>
    /// <returns>An immutable list of cache keys.</returns>
    ImmutableList<TKey> GetKeys();
}
