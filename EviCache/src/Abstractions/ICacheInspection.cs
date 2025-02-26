using System.Collections.Immutable;

namespace EviCache.Abstractions;

/// <summary>
/// Provides methods for inspecting the contents of the cache.
/// </summary>
/// <typeparam name="TKey">The type of keys in the cache.</typeparam>
/// <typeparam name="TValue">The type of values in the cache.</typeparam>
public interface ICacheInspection<TKey, TValue> : ICacheKeyProvider<TKey> where TKey : notnull
{
    /// <summary>
    /// Retrieves a snapshot of the cache as an immutable list of key/value pairs.
    /// </summary>
    /// <returns>An immutable list representing the current state of the cache.</returns>
    ImmutableList<KeyValuePair<TKey, TValue>> GetSnapshot();
}
