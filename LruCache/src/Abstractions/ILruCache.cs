namespace LruCache.Abstractions;

public interface ILruCache<TKey, TValue>
{
    TValue Get(TKey key);
    bool TryGet(TKey key, out TValue value);
    void Put(TKey key, TValue value);
    TValue GetOrAdd(TKey key, TValue value);
    TValue AddOrUpdate(TKey key, TValue value);
    bool Remove(TKey key);
    void Clear();
}
