namespace EviCache.Abstractions;

/// <summary>
/// Defines operations for interacting with the cache.
/// </summary>
/// <typeparam name="TKey">The type of keys in the cache.</typeparam>
/// <typeparam name="TValue">The type of values in the cache.</typeparam>
public interface ICacheOperations<TKey, TValue> where TKey : notnull
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
    bool ContainsKey(TKey key);

    /// <summary>
    /// Inserts an item into the cache or updates it if the key already exists.
    /// </summary>
    /// <param name="key">The key of the item to add or update.</param>
    /// <param name="value">The value to associate with the key.</param>
    void Put(TKey key, TValue value);

    /// <summary>
    /// Retrieves the value associated with the specified key, or adds the value if the key is not present.
    /// </summary>
    /// <param name="key">The key to look up or add.</param>
    /// <param name="value">The value to add if the key is not found.</param>
    /// <returns>The existing or newly added value.</returns>
    TValue GetOrAdd(TKey key, TValue value);


    /// <summary>
    /// Adds a new item or updates the value of an existing item.
    /// </summary>
    /// <param name="key">The key to add or update.</param>
    /// <param name="value">The new value to associate with the key.</param>
    /// <returns>The new value associated with the key.</returns>
    TValue AddOrUpdate(TKey key, TValue value);

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
