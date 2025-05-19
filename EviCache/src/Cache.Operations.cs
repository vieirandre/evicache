using EviCache.Abstractions;
using EviCache.Extensions;
using EviCache.Options;

namespace EviCache;

public sealed partial class Cache<TKey, TValue> : ICacheOperations<TKey, TValue> where TKey : notnull
{
    public TValue Get(TKey key)
    {
        using var _ = _gate.Lock();
        return GetCore(key);
    }

    public bool TryGet(TKey key, out TValue value)
    {
        using var _ = _gate.Lock();
        return TryGetCore(key, out value);
    }

    public bool ContainsKey(TKey key)
    {
        using var gate = _gate.Lock();
        return TryGetItem(key, out _);
    }

    public TValue GetOrAdd(TKey key, TValue value)
    {
        using var _ = _gate.Lock();
        return GetOrAddCore(key, value);
    }

    public TValue GetOrAdd(TKey key, TValue value, CacheItemOptions options)
    {
        using var _ = _gate.Lock();
        return GetOrAddCore(key, value, options);
    }

    public void Put(TKey key, TValue value)
    {
        using var _ = _gate.Lock();
        PutCore(key, value);
    }

    public void Put(TKey key, TValue value, CacheItemOptions options)
    {
        using var _ = _gate.Lock();
        PutCore(key, value, options);
    }

    public TValue AddOrUpdate(TKey key, TValue value)
    {
        using var _ = _gate.Lock();
        return AddOrUpdateCore(key, value);
    }

    public TValue AddOrUpdate(TKey key, TValue value, CacheItemOptions options)
    {
        using var _ = _gate.Lock();
        return AddOrUpdateCore(key, value, options);
    }

    public bool Remove(TKey key)
    {
        using var _ = _gate.Lock();
        return RemoveCore(key);
    }

    public void Clear()
    {
        using var _ = _gate.Lock();
        ClearCore();
    }
}