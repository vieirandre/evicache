namespace EviCache.Abstractions;

public interface ICacheOperations<TKey, TValue> where TKey : notnull
{
    TValue Get(TKey key);
    bool TryGet(TKey key, out TValue value);
    bool ContainsKey(TKey key);

    void Put(TKey key, TValue value);
    TValue GetOrAdd(TKey key, TValue value);
    TValue AddOrUpdate(TKey key, TValue value);

    bool Remove(TKey key);
    void Clear();
}
