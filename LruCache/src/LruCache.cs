namespace LruCache;

public class LruCache<TKey, TValue> : ILruCache<TKey, TValue>
{
    private readonly int _capacity;

    public LruCache(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

        _capacity = capacity;
    }

    public TValue Get(TKey key)
    {
        throw new NotImplementedException();
    }

    public void Put(TKey key, TValue value)
    {
        throw new NotImplementedException();
    }
}
