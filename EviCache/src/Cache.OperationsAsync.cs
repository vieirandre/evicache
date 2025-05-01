using EviCache.Abstractions;
using EviCache.Options;

namespace EviCache;

public sealed partial class Cache<TKey, TValue> : ICacheOperationsAsync<TKey, TValue> where TKey : notnull
{
    public Task<TValue> GetAsync(TKey key, CancellationToken ct = default)
        => WithLockAsync(() => GetCore(key), ct);

    public Task<(bool Found, TValue Value)> TryGetAsync(TKey key, CancellationToken ct = default)
        => WithLockAsync(() =>
        {
            bool found = TryGetCore(key, out var value);
            return (found, value);
        }, ct);

    public Task<bool> ContainsKeyAsync(TKey key, CancellationToken ct = default)
        => WithLockAsync(() => ContainsKeyCore(key), ct);

    public Task PutAsync(TKey key, TValue value, CancellationToken ct = default)
        => WithLockAsync(() => PutCore(key, value), ct);

    public Task PutAsync(TKey key, TValue value, CacheItemOptions options, CancellationToken ct = default)
        => WithLockAsync(() => PutCore(key, value, options), ct);

    public Task<TValue> GetOrAddAsync(TKey key, TValue value, CancellationToken ct = default)
        => WithLockAsync(() => GetOrAddCore(key, value), ct);

    public Task<TValue> GetOrAddAsync(TKey key, TValue value, CacheItemOptions options, CancellationToken ct = default)
        => WithLockAsync(() => GetOrAddCore(key, value, options), ct);

    public Task<TValue> AddOrUpdateAsync(TKey key, TValue value, CancellationToken ct = default)
        => WithLockAsync(() => AddOrUpdateCore(key, value), ct);

    public Task<TValue> AddOrUpdateAsync(TKey key, TValue value, CacheItemOptions options, CancellationToken ct = default)
        => WithLockAsync(() => AddOrUpdateCore(key, value, options), ct);

    public Task<bool> RemoveAsync(TKey key, CancellationToken ct = default)
        => WithLockAsync(() => RemoveCore(key), ct);

    public Task ClearAsync(CancellationToken ct = default)
        => WithLockAsync(() => ClearCore(ct), ct);
}