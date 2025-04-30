using EviCache.Exceptions;
using EviCache.Options;

namespace EviCache.Abstractions;

/// <summary>
/// Defines asynchronous operations for interacting with the cache.
/// </summary>
/// <typeparam name="TKey">The type of keys in the cache.</typeparam>
/// <typeparam name="TValue">The type of values in the cache.</typeparam>
public interface ICacheOperationsAsync<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// Asynchronously retrieves the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to retrieve.</param>
    /// <returns>A task representing the asynchronous operation, containing the value associated with the key.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the key is not found in the cache.</exception>
    Task<TValue> GetAsync(TKey key, CancellationToken ct);

    /// <summary>
    /// Asynchronously attempts to retrieve the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <returns>A task representing the asynchronous operation, with a tuple indicating whether the key was found and its associated value.</returns>
    Task<(bool Found, TValue Value)> TryGetAsync(TKey key, CancellationToken ct);

    /// <summary>
    /// Asynchronously determines whether the cache contains the specified key.
    /// </summary>
    /// <param name="key">The key to check for existence.</param>
    /// <returns>A task representing the asynchronous operation, containing a boolean that indicates whether the key exists.</returns>
    /// <remarks>
    /// Unlike other retrieval methods, this one does not trigger cache hit or miss counters.
    /// </remarks>
    Task<bool> ContainsKeyAsync(TKey key, CancellationToken ct);

    /// <summary>
    /// Asynchronously inserts an item into the cache or updates it if the key already exists.
    /// </summary>
    /// <param name="key">The key of the item to add or update.</param>
    /// <param name="value">The value to associate with the key.</param>
    /// <param name="options">Settings for the cache item.</param>
    /// <exception cref="CacheFullException">Thrown if the cache is full and unable to add the new item.</exception>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Unlike other insertion methods, this one does not trigger cache hit or miss counters.
    /// </remarks>
    Task PutAsync(TKey key, TValue value, CancellationToken ct);

    /// <inheritdoc cref="ICacheOperationsAsync{TKey, TValue}.PutAsync(TKey, TValue)" />
    Task PutAsync(TKey key, TValue value, CacheItemOptions options, CancellationToken ct);

    /// <summary>
    /// Asynchronously retrieves the value associated with the specified key, or adds the value if the key is not present.
    /// </summary>
    /// <param name="key">The key to look up or add.</param>
    /// <param name="value">The value to add if the key is not found.</param>
    /// <param name="options">Settings for the cache item.</param>
    /// <returns>A task representing the asynchronous operation, containing the existing or newly added value.</returns>
    /// <exception cref="CacheFullException">Thrown if the cache is full and unable to add the new item.</exception>
    Task<TValue> GetOrAddAsync(TKey key, TValue value, CancellationToken ct);

    /// <inheritdoc cref="ICacheOperationsAsync{TKey, TValue}.GetOrAddAsync(TKey, TValue)" />
    Task<TValue> GetOrAddAsync(TKey key, TValue value, CacheItemOptions options, CancellationToken ct);

    /// <summary>
    /// Asynchronously adds a new item or updates the value of an existing item.
    /// </summary>
    /// <param name="key">The key to add or update.</param>
    /// <param name="value">The new value to associate with the key.</param>
    /// <param name="options">Settings for the cache item.</param>
    /// <exception cref="CacheFullException">Thrown if the cache is full and unable to add the new item.</exception>
    /// <returns>A task representing the asynchronous operation, containing the new value.</returns>
    Task<TValue> AddOrUpdateAsync(TKey key, TValue value, CancellationToken ct);

    /// <inheritdoc cref="ICacheOperationsAsync{TKey, TValue}.AddOrUpdateAsync(TKey, TValue)" />
    Task<TValue> AddOrUpdateAsync(TKey key, TValue value, CacheItemOptions options, CancellationToken ct);

    /// <summary>
    /// Asynchronously removes the item with the specified key from the cache.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    /// <returns>A task representing the asynchronous operation, containing a boolean that indicates whether the removal was successful.</returns>
    Task<bool> RemoveAsync(TKey key, CancellationToken ct);

    /// <summary>
    /// Asynchronously clears all items from the cache.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ClearAsync(CancellationToken ct);
}
