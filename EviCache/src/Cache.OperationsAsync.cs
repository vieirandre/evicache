using EviCache.Abstractions;
using EviCache.Extensions;
using EviCache.Options;

namespace EviCache;

public sealed partial class Cache<TKey, TValue> : ICacheOperationsAsync<TKey, TValue> where TKey : notnull
{
    public Task<TValue> GetAsync(TKey key, CancellationToken ct = default)
        => _gate.ExecuteAsync(() => GetCore(key), ct);

    public Task<(bool Found, TValue Value)> TryGetAsync(TKey key, CancellationToken ct = default)
        => _gate.ExecuteAsync(() =>
        {
            bool found = TryGetCore(key, out var value);
            return (found, value);
        }, ct);

    public Task<bool> ContainsKeyAsync(TKey key, CancellationToken ct = default)
        => _gate.ExecuteAsync(() => ContainsKeyCore(key), ct);

    public Task PutAsync(TKey key, TValue value, CancellationToken ct = default)
        => _gate.ExecuteAsync(() => PutCore(key, value), ct);

    public Task PutAsync(TKey key, TValue value, CacheItemOptions options, CancellationToken ct = default)
        => _gate.ExecuteAsync(() => PutCore(key, value, options), ct);

    public Task<TValue> GetOrAddAsync(TKey key, TValue value, CancellationToken ct = default)
        => _gate.ExecuteAsync(() => GetOrAddCore(key, value), ct);

    public Task<TValue> GetOrAddAsync(TKey key, TValue value, CacheItemOptions options, CancellationToken ct = default)
        => _gate.ExecuteAsync(() => GetOrAddCore(key, value, options), ct);

    public Task<TValue> AddOrUpdateAsync(TKey key, TValue value, CancellationToken ct = default)
        => _gate.ExecuteAsync(() => AddOrUpdateCore(key, value), ct);

    public Task<TValue> AddOrUpdateAsync(TKey key, TValue value, CacheItemOptions options, CancellationToken ct = default)
        => _gate.ExecuteAsync(() => AddOrUpdateCore(key, value, options), ct);

    public Task<bool> RemoveAsync(TKey key, CancellationToken ct = default)
        => _gate.ExecuteAsync(() => RemoveCore(key), ct);

    public Task ClearAsync(CancellationToken ct = default)
        => _gate.ExecuteAsync(() => ClearCore(ct), ct);
}