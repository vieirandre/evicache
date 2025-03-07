namespace EviCache.Exceptions;

/// <summary>
/// The exception thrown when the cache is full and unable to add new items.
/// </summary>
public class CacheFullException : Exception
{
    /// <summary>
    /// Gets the capacity of the cache at the time the exception was thrown.
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheFullException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public CacheFullException(string message) : base(message)
    {
        Capacity = -1;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheFullException"/> class with a specified error message and capacity.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="capacity">The capacity of the cache when the exception occurred.</param>
    public CacheFullException(string message, int capacity) : base(message)
    {
        Capacity = capacity;
    }
}
