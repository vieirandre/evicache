﻿using EviCache.Exceptions;
using EviCache.Options;

namespace EviCache.Abstractions;

/// <summary>
/// Defines operations for interacting with the cache.
/// </summary>
/// <typeparam name="TKey">The type of keys in the cache.</typeparam>
/// <typeparam name="TValue">The type of values in the cache.</typeparam>
public interface ICacheOperations<in TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// Retrieves the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to retrieve.</param>
    /// <returns>The value associated with the key.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the key is not found in the cache.</exception>
    TValue Get(TKey key);

    /// <summary>
    /// Attempts to retrieve the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <param name="value">When this method returns, contains the value associated with the key, if found; otherwise, the default value for the type.</param>
    /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
    bool TryGet(TKey key, out TValue value);

    /// <summary>
    /// Determines whether the cache contains the specified key.
    /// </summary>
    /// <param name="key">The key to check for existence.</param>
    /// <returns><c>true</c> if the key exists in the cache; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Unlike other retrieval methods, this one does not trigger cache hit or miss counters.
    /// </remarks>
    bool ContainsKey(TKey key);

    /// <summary>
    /// Inserts an item into the cache or updates it if the key already exists.
    /// </summary>
    /// <param name="key">The key of the item to add or update.</param>
    /// <param name="value">The value to associate with the key.</param>
    /// <param name="options">Settings for the cache item.</param>
    /// <exception cref="CacheFullException">Thrown if the cache is full and unable to add the new item.</exception>
    /// <remarks>
    /// Unlike other insertion methods, this one does not trigger cache hit or miss counters.
    /// </remarks>
    void Put(TKey key, TValue value);

    /// <inheritdoc cref="ICacheOperations{TKey, TValue}.Put(TKey, TValue)" />
    void Put(TKey key, TValue value, CacheItemOptions options);

    /// <summary>
    /// Retrieves the value associated with the specified key, or adds the value if the key is not present.
    /// </summary>
    /// <param name="key">The key to look up or add.</param>
    /// <param name="value">The value to add if the key is not found.</param>
    /// <param name="options">Settings for the cache item.</param>
    /// <returns>The existing or newly added value.</returns>
    /// <exception cref="CacheFullException">Thrown if the cache is full and unable to add the new item.</exception>
    TValue GetOrAdd(TKey key, TValue value);

    /// <inheritdoc cref="ICacheOperations{TKey, TValue}.GetOrAdd(TKey, TValue)" />
    TValue GetOrAdd(TKey key, TValue value, CacheItemOptions options);

    /// <summary>
    /// Adds a new item or updates the value of an existing item.
    /// </summary>
    /// <param name="key">The key to add or update.</param>
    /// <param name="value">The new value to associate with the key.</param>
    /// <param name="options">Settings for the cache item.</param>
    /// <returns>The new value associated with the key.</returns>
    /// <exception cref="CacheFullException">Thrown if the cache is full and unable to add the new item.</exception>
    TValue AddOrUpdate(TKey key, TValue value);

    /// <inheritdoc cref="ICacheOperations{TKey, TValue}.AddOrUpdate(TKey, TValue)" />
    TValue AddOrUpdate(TKey key, TValue value, CacheItemOptions options);

    /// <summary>
    /// Removes the item with the specified key from the cache.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    /// <returns><c>true</c> if the item was removed; otherwise, <c>false</c>.</returns>
    bool Remove(TKey key);

    /// <summary>
    /// Clears all items from the cache.
    /// </summary>
    void Clear();
}
