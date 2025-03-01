using EviCache.Abstractions;
using Microsoft.Extensions.Logging;

namespace EviCache;

public partial class Cache<TKey, TValue> : ICacheOperations<TKey, TValue> where TKey : notnull
{
    public TValue Get(TKey key)
    {
        lock (_syncLock)
        {
            if (_cacheMap.TryGetValue(key, out var value))
            {
                Interlocked.Increment(ref _hits);

                _cacheHandler.RegisterAccess(key);
                return value;
            }

            Interlocked.Increment(ref _misses);

            throw new KeyNotFoundException($"The key '{key}' wasn't found in the cache");
        }
    }

    public bool TryGet(TKey key, out TValue value)
    {
        lock (_syncLock)
        {
            if (_cacheMap.TryGetValue(key, out value))
            {
                Interlocked.Increment(ref _hits);

                _cacheHandler.RegisterAccess(key);
                return true;
            }

            Interlocked.Increment(ref _misses);

            value = default!;
            return false;
        }
    }

    public bool ContainsKey(TKey key)
    {
        lock (_syncLock)
        {
            return _cacheMap.ContainsKey(key);
        }
    }

    public void Put(TKey key, TValue value)
    {
        lock (_syncLock)
        {
            if (_cacheMap.ContainsKey(key))
            {
                _cacheMap[key] = value;
                _cacheHandler.RegisterUpdate(key);

                return;
            }

            if (_cacheMap.Count >= _capacity)
                Evict();

            AddNewItem(key, value);
        }
    }

    public TValue GetOrAdd(TKey key, TValue value)
    {
        lock (_syncLock)
        {
            if (_cacheMap.TryGetValue(key, out var existing))
            {
                Interlocked.Increment(ref _hits);

                _cacheHandler.RegisterAccess(key);
                return existing;
            }

            Interlocked.Increment(ref _misses);

            if (_cacheMap.Count >= _capacity)
                Evict();

            AddNewItem(key, value);
            return value;
        }
    }

    public TValue AddOrUpdate(TKey key, TValue value)
    {
        lock (_syncLock)
        {
            if (_cacheMap.ContainsKey(key))
            {
                Interlocked.Increment(ref _hits);

                _cacheMap[key] = value;
                _cacheHandler.RegisterUpdate(key);

                return value;
            }

            Interlocked.Increment(ref _misses);

            if (_cacheMap.Count >= _capacity)
                Evict();

            AddNewItem(key, value);
            return value;
        }
    }

    public bool Remove(TKey key)
    {
        lock (_syncLock)
        {
            if (!_cacheMap.TryGetValue(key, out var value))
                return false;

            _cacheMap.Remove(key);
            _cacheHandler.RegisterRemoval(key);

            DisposeItem(value);

            return true;
        }
    }

    public void Clear()
    {
        int removedCount;
        List<IDisposable> disposables;

        lock (_syncLock)
        {
            removedCount = _cacheMap.Count;

            disposables = _cacheMap.Values
                .OfType<IDisposable>()
                .ToList();

            _cacheMap.Clear();
            _cacheHandler.Clear();
        }

        _ = Task.Run(() =>
        {
            foreach (var disposable in disposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while disposing cache item in the background");
                }
            }
        });

        _logger.LogInformation("Cache cleared. Removed {Count} items", removedCount);
    }

    private void AddNewItem(TKey key, TValue value)
    {
        _cacheMap[key] = value;
        _cacheHandler.RegisterInsertion(key);
    }

    private void Evict()
    {
        if (!_cacheHandler.TrySelectEvictionCandidate(out var candidate))
        {
            _logger.LogError("Eviction handler didn't return a candidate");
            return;
        }

        if (!_cacheMap.TryGetValue(candidate, out var value))
        {
            _logger.LogError("Eviction candidate ({Candidate}) wasn't found in the cache", candidate);
            return;
        }

        _cacheMap.Remove(candidate);
        _cacheHandler.RegisterRemoval(candidate);

        Interlocked.Increment(ref _evictions);
        _logger.LogDebug("Evicted key from cache: {Key} | Total evictions: {Evictions}", candidate, _evictions);

        DisposeItem(value);
    }
}