namespace EviCache.Exceptions;

public class CacheFullException : Exception
{
    public int Capacity { get; }

    public CacheFullException(string message) : base(message)
    {
        Capacity = -1;
    }

    public CacheFullException(string message, int capacity) : base(message)
    {
        Capacity = capacity;
    }
}
