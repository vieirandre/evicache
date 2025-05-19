using EviCache.Exceptions;
using EviCache.Models;
using EviCache.Options;
using Microsoft.Extensions.Logging;

namespace EviCache;

public sealed partial class Cache<TKey, TValue> where TKey : notnull
{
    private TValue GetCore(TKey key)
    {
        if (TryGetItem(key, out var item))
        {
            Interlocked.Increment(ref _hits);

            item.Metadata.RegisterAccess();
            _cacheHandler.RegisterAccess(key);

            return item.Value;
        }

        Interlocked.Increment(ref _misses);

        throw new KeyNotFoundException($"The key '{key}' was not found in the cache");
    }

    private bool TryGetCore(TKey key, out TValue value)
    {
        if (TryGetItem(key, out var item))
        {
            Interlocked.Increment(ref _hits);

            value = item.Value;
            item.Metadata.RegisterAccess();
            _cacheHandler.RegisterAccess(key);

            return true;
        }

        Interlocked.Increment(ref _misses);

        value = default!;
        return false;
    }

    private bool ContainsKeyCore(TKey key) => TryGetItem(key, out _);

    private TValue GetOrAddCore(TKey key, TValue value)
    {
        if (TryGetItem(key, out var item))
        {
            Interlocked.Increment(ref _hits);

            item.Metadata.RegisterAccess();
            _cacheHandler.RegisterAccess(key);

            return item.Value;
        }

        Interlocked.Increment(ref _misses);

        EnsureCapacityForKey(key);
        AddOrUpdateItem(key, value, isUpdate: false);

        return value;
    }

    private TValue GetOrAddCore(TKey key, TValue value, CacheItemOptions options)
    {
        if (TryGetItem(key, out var item))
        {
            Interlocked.Increment(ref _hits);

            item.Metadata.RegisterAccess();
            _cacheHandler.RegisterAccess(key);

            return item.Value;
        }

        Interlocked.Increment(ref _misses);

        EnsureCapacityForKey(key);
        AddOrUpdateItem(key, value, options, isUpdate: false);

        return value;
    }

    private void PutCore(TKey key, TValue value)
    {
        if (TryGetItem(key, out _))
        {
            AddOrUpdateItem(key, value, isUpdate: true);
            return;
        }

        EnsureCapacityForKey(key);
        AddOrUpdateItem(key, value, isUpdate: false);
    }

    private void PutCore(TKey key, TValue value, CacheItemOptions options)
    {
        if (TryGetItem(key, out _))
        {
            AddOrUpdateItem(key, value, options, isUpdate: true);
            return;
        }

        EnsureCapacityForKey(key);
        AddOrUpdateItem(key, value, options, isUpdate: false);
    }

    private TValue AddOrUpdateCore(TKey key, TValue value)
    {
        if (TryGetItem(key, out _))
        {
            Interlocked.Increment(ref _hits);

            AddOrUpdateItem(key, value, isUpdate: true);
        }
        else
        {
            Interlocked.Increment(ref _misses);

            EnsureCapacityForKey(key);
            AddOrUpdateItem(key, value, isUpdate: false);
        }

        return value;
    }

    private TValue AddOrUpdateCore(TKey key, TValue value, CacheItemOptions options)
    {
        if (TryGetItem(key, out _))
        {
            Interlocked.Increment(ref _hits);

            AddOrUpdateItem(key, value, options, isUpdate: true);
        }
        else
        {
            Interlocked.Increment(ref _misses);

            EnsureCapacityForKey(key);
            AddOrUpdateItem(key, value, options, isUpdate: false);
        }

        return value;
    }

    private bool RemoveCore(TKey key)
    {
        if (!TryGetItem(key, out var item))
            return false;

        RemoveItem(key, item);

        return true;
    }

    private void ClearCore(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        int removedCount = _cacheMap.Count;

        List<object> disposables = _cacheMap.Values
            .Select(ci => ci.Value)
            .Where(v => v is not null && (v is IDisposable or IAsyncDisposable))
            .Cast<object>()
            .ToList();

        _cacheMap.Clear();
        _cacheHandler.Clear();

        _ = Task.Run(async () =>
        {
            foreach (var disposable in disposables)
            {
                if (ct.IsCancellationRequested)
                    break;

                try
                {
                    await DisposeValueAsync(disposable);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while disposing cache item in the background");
                }
            }
        }, ct);

        _logger.LogInformation("Cache cleared. Removed {Count} items", removedCount);
    }

    private bool TryGetItem(TKey key, out CacheItem<TValue> item)
    {
        if (!_cacheMap.TryGetValue(key, out item!))
            return false;

        if (IsExpired(key, item))
            return false;

        return true;
    }

    private bool IsExpired(TKey key, CacheItem<TValue> item)
    {
        if (item.Metadata.IsExpired)
        {
            RemoveItem(key, item);
            return true;
        }

        return false;
    }

    private void AddOrUpdateItem(TKey key, TValue value, bool isUpdate)
    {
        bool defaultExpirationExists = _defaultExpiration is not null;

        if (isUpdate)
        {
            var item = _cacheMap[key];
            item.UpdateItem(value);

            _cacheHandler.RegisterUpdate(key);
        }
        else
        {
            _cacheMap.Add(key,
                defaultExpirationExists
                ? new CacheItem<TValue>(value, new CacheItemOptions { Expiration = _defaultExpiration })
                : new CacheItem<TValue>(value));

            _cacheHandler.RegisterInsertion(key);
        }
    }

    private void AddOrUpdateItem(TKey key, TValue value, CacheItemOptions options, bool isUpdate)
    {
        if (isUpdate)
        {
            var item = _cacheMap[key];
            item.UpdateItem(value, options);
            _cacheHandler.RegisterUpdate(key);
        }
        else
        {
            _cacheMap.Add(key, new CacheItem<TValue>(value, options));
            _cacheHandler.RegisterInsertion(key);
        }
    }

    private void RemoveItem(TKey key, CacheItem<TValue> item)
    {
        _cacheMap.Remove(key);
        _cacheHandler.RegisterRemoval(key);

        DisposeItem(item);
    }

    private void EnsureCapacityForKey(TKey key)
    {
        if (_cacheMap.Count < _capacity)
            return;

        if (_evictionCandidateSelector is null)
            throw new CacheFullException(
                $"Cache is full (capacity: {_capacity}) and uses {_evictionPolicy} policy",
                _capacity,
                key!.ToString(),
                _evictionPolicy
            );

        if (!TryEvictItem())
            throw new CacheFullException(
                $"Cache is full (capacity: {_capacity}). Failed to evict any item while adding key: {key}",
                _capacity,
                key!.ToString(),
                _evictionPolicy
            );
    }

    private bool TryEvictItem()
    {
        int attempts = 0;
        int maxAttempts = _cacheMap.Count;

        while (attempts < maxAttempts)
        {
            attempts++;

            if (_evictionCandidateSelector?.TrySelectEvictionCandidate(out var candidate) != true)
            {
                _logger.LogError("Eviction selector did not return a candidate");
                return false;
            }

            if (!_cacheMap.TryGetValue(candidate, out var item))
            {
                _logger.LogError("Eviction candidate ({Candidate}) was not found in the cache", candidate);
                continue;
            }

            // purged but not counted as eviction
            if (IsExpired(candidate, item))
                continue;

            RemoveItem(candidate, item);

            Interlocked.Increment(ref _evictions);

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Evicted key from cache: {Key} | Total evictions: {Evictions}", candidate, _evictions);

            return true;
        }

        return false;
    }
}
