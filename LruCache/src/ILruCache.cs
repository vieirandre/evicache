namespace LruCache;

public interface ILruCache<TKey, TValue>
{
    TValue Get(TKey key);
    void Put(TKey key, TValue value);
}
