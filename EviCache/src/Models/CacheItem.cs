namespace EviCache.Models;

public class CacheItem<TKey, TValue>(TKey key, TValue value)
{
    public TKey Key { get; } = key;
    public TValue Value { get; set; } = value;
}
