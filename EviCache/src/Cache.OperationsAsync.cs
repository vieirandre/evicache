using EviCache.Abstractions;
using EviCache.Extensions;
using EviCache.Options;

namespace EviCache;

public sealed partial class Cache<TKey, TValue> : ICacheOperationsAsync<TKey, TValue> where TKey : notnull
{
    public async Task<TValue> GetAsync(TKey key, CancellationToken ct = default)
    {
        await using var _ = await _gate.LockAsync(ct);
        return GetCore(key);
    }

    public async Task<(bool Found, TValue Value)> TryGetAsync(TKey key, CancellationToken ct = default)
    {
        await using var _ = await _gate.LockAsync(ct);
        bool found = TryGetCore(key, out var value);
        return (found, value);
    }

    public async Task<bool> ContainsKeyAsync(TKey key, CancellationToken ct = default)
    {
        await using var _ = await _gate.LockAsync(ct);
        return ContainsKeyCore(key);
    }

    public async Task PutAsync(TKey key, TValue value, CancellationToken ct = default)
    {
        await using var _ = await _gate.LockAsync(ct);
        PutCore(key, value);
    }

    public async Task PutAsync(TKey key, TValue value, CacheItemOptions options, CancellationToken ct = default)
    {
        await using var _ = await _gate.LockAsync(ct);
        PutCore(key, value, options);
    }

    public async Task<TValue> GetOrAddAsync(TKey key, TValue value, CancellationToken ct = default)
    {
        await using var _ = await _gate.LockAsync(ct);
        return GetOrAddCore(key, value);
    }

    public async Task<TValue> GetOrAddAsync(TKey key, TValue value, CacheItemOptions options, CancellationToken ct = default)
    {
        await using var _ = await _gate.LockAsync(ct);
        return GetOrAddCore(key, value, options);
    }

    public async Task<TValue> AddOrUpdateAsync(TKey key, TValue value, CancellationToken ct = default)
    {
        await using var _ = await _gate.LockAsync(ct);
        return AddOrUpdateCore(key, value);
    }

    public async Task<TValue> AddOrUpdateAsync(TKey key, TValue value, CacheItemOptions options, CancellationToken ct = default)
    {
        await using var _ = await _gate.LockAsync(ct);
        return AddOrUpdateCore(key, value, options);
    }

    public async Task<bool> RemoveAsync(TKey key, CancellationToken ct = default)
    {
        await using var _ = await _gate.LockAsync(ct);
        return RemoveCore(key);
    }

    public async Task ClearAsync(CancellationToken ct = default)
    {
        await using var _ = await _gate.LockAsync(ct);
        ClearCore(ct);
    }
}
