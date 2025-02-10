using EviCache.Abstractions;
using EviCache.Models;

namespace EviCache;

public partial class EviCache<TKey, TValue> : ICacheOperations<TKey, TValue>, ICacheMetrics, ICacheUtils<TKey, TValue>, IDisposable where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, LinkedListNode<CacheItem<TKey, TValue>>> _cacheMap;
    private readonly LinkedList<CacheItem<TKey, TValue>> _lruList;
    private readonly object _syncLock = new();

    public EviCache(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

        _capacity = capacity;
        _cacheMap = new Dictionary<TKey, LinkedListNode<CacheItem<TKey, TValue>>>(capacity);
        _lruList = new LinkedList<CacheItem<TKey, TValue>>();
    }

    public TValue Get(TKey key)
    {
        lock (_syncLock)
        {
            if (_cacheMap.TryGetValue(key, out var node))
            {
                Interlocked.Increment(ref _hits);

                MoveToFront(node);
                return node.Value.Value;
            }

            Interlocked.Increment(ref _misses);

            throw new KeyNotFoundException($"The key '{key}' wasn't found in the cache");
        }
    }

    public bool TryGet(TKey key, out TValue value)
    {
        lock (_syncLock)
        {
            if (_cacheMap.TryGetValue(key, out var node))
            {
                Interlocked.Increment(ref _hits);

                MoveToFront(node);
                value = node.Value.Value;
                return true;
            }

            Interlocked.Increment(ref _misses);

            value = default;
            return false;
        }
    }

    public void Put(TKey key, TValue value)
    {
        lock (_syncLock)
        {
            if (_cacheMap.TryGetValue(key, out var existingNode))
            {
                existingNode.Value.Value = value;
                MoveToFront(existingNode);
            }
            else
            {
                if (_cacheMap.Count >= _capacity)
                    EvictLeastRecentlyUsed();

                AddNewNode(key, value);
            }
        }
    }

    public TValue GetOrAdd(TKey key, TValue value)
    {
        lock (_syncLock)
        {
            if (_cacheMap.TryGetValue(key, out var node))
            {
                Interlocked.Increment(ref _hits);

                MoveToFront(node);
                return node.Value.Value;
            }

            Interlocked.Increment(ref _misses);

            if (_cacheMap.Count >= _capacity)
                EvictLeastRecentlyUsed();

            AddNewNode(key, value);

            return value;
        }
    }

    public TValue AddOrUpdate(TKey key, TValue value)
    {
        lock (_syncLock)
        {
            if (_cacheMap.TryGetValue(key, out var node))
            {
                Interlocked.Increment(ref _hits);

                node.Value.Value = value;
                MoveToFront(node);

                return value;
            }

            Interlocked.Increment(ref _misses);

            if (_cacheMap.Count >= _capacity)
                EvictLeastRecentlyUsed();

            AddNewNode(key, value);

            return value;
        }
    }

    public bool Remove(TKey key)
    {
        lock (_syncLock)
        {
            if (_cacheMap.TryGetValue(key, out var node))
            {
                _cacheMap.Remove(key);
                _lruList.Remove(node);

                DisposeItem(node);

                return true;
            }

            return false;
        }
    }

    private void MoveToFront(LinkedListNode<CacheItem<TKey, TValue>> node)
    {
        _lruList.Remove(node);
        _lruList.AddFirst(node);
    }

    private void EvictLeastRecentlyUsed()
    {
        var lruNode = _lruList.Last;

        if (lruNode is null)
            return;

        _cacheMap.Remove(lruNode.Value.Key);
        _lruList.RemoveLast();

        Interlocked.Increment(ref _evictions);

        DisposeItem(lruNode);
    }

    private void AddNewNode(TKey key, TValue value)
    {
        var newItem = new CacheItem<TKey, TValue>(key, value);
        var newNode = new LinkedListNode<CacheItem<TKey, TValue>>(newItem);

        _lruList.AddFirst(newNode);
        _cacheMap[key] = newNode;
    }
}
