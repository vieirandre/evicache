using EviCache.Abstractions;
using EviCache.Options;

namespace EviCache;

public sealed partial class Cache<TKey, TValue> : ICacheOperations<TKey, TValue> where TKey : notnull
{
    public TValue Get(TKey key)
        => WithLock(() => GetCore(key));

    public bool TryGet(TKey key, out TValue value)
    {
        TValue tmp = default!;

        bool found = WithLock(() => TryGetCore(key, out tmp));

        value = tmp;
        return found;
    }

    public bool ContainsKey(TKey key)
        => WithLock(() => TryGetItem(key, out _));

    public TValue GetOrAdd(TKey key, TValue value)
        => WithLock(() => GetOrAddCore(key, value));

    public TValue GetOrAdd(TKey key, TValue value, CacheItemOptions options)
        => WithLock(() => GetOrAddCore(key, value, options));

    public void Put(TKey key, TValue value)
        => WithLock(() => PutCore(key, value));

    public void Put(TKey key, TValue value, CacheItemOptions options)
        => WithLock(() => PutCore(key, value, options));

    public TValue AddOrUpdate(TKey key, TValue value)
        => WithLock(() => AddOrUpdateCore(key, value));

    public TValue AddOrUpdate(TKey key, TValue value, CacheItemOptions options)
        => WithLock(() => AddOrUpdateCore(key, value, options));

    public bool Remove(TKey key)
        => WithLock(() => RemoveCore(key));

    public void Clear()
        => WithLock(() => ClearCore());
}