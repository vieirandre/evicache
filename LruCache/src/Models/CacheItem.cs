namespace LruCache.Models;

public class CacheItem<TKey, TValue>
{
    public TKey Key { get; set; }
    public TValue Value { get; set; }

    public CacheItem(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }
}
