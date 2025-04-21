using EviCache.Enums;

namespace EviCache.Exceptions;

/// <summary>
/// The exception thrown when the cache is full and unable to add new items.
/// </summary>
public class CacheFullException : Exception
{
    /// <summary>
    /// Gets the capacity of the cache at the time the exception was thrown.
    /// </summary>
    /// <remarks>
    /// A capacity value of -1 indicates that the cache capacity was not specified or is unavailable.
    /// </remarks>
    public int Capacity { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheFullException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <remarks>
    /// Sets Capacity to -1 to indicate it was not specified.
    /// </remarks>
    internal CacheFullException(string message) : base(message)
    {
        Capacity = -1;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheFullException"/> class with a specified error message, capacity, the key that failed to be added, and the eviction policy.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="capacity">The capacity of the cache when the exception occurred.</param>
    /// <param name="attemptedKey">The key of the item that was attempted to be added to the full cache. Can be null if the key is not available or not relevant.</param>
    /// <param name="policy">The eviction policy in effect when the cache became full.</param>
    /// <remarks>
    /// This constructor stores the <paramref name="capacity"/>, the <paramref name="attemptedKey"/> (if provided), and the <paramref name="policy"/> 
    /// in the exception's <see cref="Exception.Data"/> dictionary for additional diagnostic information.
    /// </remarks>
    internal CacheFullException(string message, int capacity, string? attemptedKey, EvictionPolicy policy) : base(message)
    {
        Capacity = capacity;
        Data["Capacity"] = capacity;

        if (attemptedKey is not null)
            Data["AttemptedKey"] = attemptedKey;

        Data["EvictionPolicy"] = policy.ToString();
    }
}
