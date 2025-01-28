namespace LruCache;

public interface ILruCache<TKey, TValue>
{
    bool TryGet(TKey key, out TValue value);
    void Put(TKey key, TValue value);
    int Count { get; }
}
