using EviCache.Abstractions;
using EviCache.Extensions;
using EviCache.Options;

namespace EviCache;

public sealed partial class Cache<TKey, TValue> : ICacheOperations<TKey, TValue> where TKey : notnull
{
    public TValue Get(TKey key)
        => _gate.Execute(() => GetCore(key));

    public bool TryGet(TKey key, out TValue value)
    {
        TValue tmp = default!;

        bool found = _gate.Execute(() => TryGetCore(key, out tmp));

        value = tmp;
        return found;
    }

    public bool ContainsKey(TKey key)
        => _gate.Execute(() => TryGetItem(key, out _));

    public TValue GetOrAdd(TKey key, TValue value)
        => _gate.Execute(() => GetOrAddCore(key, value));

    public TValue GetOrAdd(TKey key, TValue value, CacheItemOptions options)
        => _gate.Execute(() => GetOrAddCore(key, value, options));

    public void Put(TKey key, TValue value)
        => _gate.Execute(() => PutCore(key, value));

    public void Put(TKey key, TValue value, CacheItemOptions options)
        => _gate.Execute(() => PutCore(key, value, options));

    public TValue AddOrUpdate(TKey key, TValue value)
        => _gate.Execute(() => AddOrUpdateCore(key, value));

    public TValue AddOrUpdate(TKey key, TValue value, CacheItemOptions options)
        => _gate.Execute(() => AddOrUpdateCore(key, value, options));

    public bool Remove(TKey key)
        => _gate.Execute(() => RemoveCore(key));

    public void Clear()
        => _gate.Execute(() => ClearCore());
}