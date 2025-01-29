﻿using LruCache.Models;

namespace LruCache;

public class LruCache<TKey, TValue> : ILruCache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, LinkedListNode<CacheItem<TKey, TValue>>> _cacheMap;
    private readonly LinkedList<CacheItem<TKey, TValue>> _lruList;

    private readonly object _syncLock = new();

    public LruCache(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

        _capacity = capacity;
        _cacheMap = new Dictionary<TKey, LinkedListNode<CacheItem<TKey, TValue>>>(capacity);
        _lruList = new LinkedList<CacheItem<TKey, TValue>>();
    }

    public int Count
    {
        get { lock (_syncLock) { return _cacheMap.Count; } }
    }

    public bool TryGet(TKey key, out TValue value)
    {
        lock (_syncLock)
        {
            if (_cacheMap.TryGetValue(key, out var node))
            {
                MoveToFront(node);

                value = node.Value.Value;
                return true;
            }

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

                var newItem = new CacheItem<TKey, TValue>(key, value);
                var newNode = new LinkedListNode<CacheItem<TKey, TValue>>(newItem);

                _lruList.AddFirst(newNode);
                _cacheMap[key] = newNode;
            }
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

    public void Clear()
    {
        lock (_syncLock)
        {
            foreach (var entry in _cacheMap)
            {
                DisposeItem(entry.Value);
            }

            _cacheMap.Clear();
            _lruList.Clear();
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

        DisposeItem(lruNode);
    }

    private static void DisposeItem(LinkedListNode<CacheItem<TKey, TValue>> node)
    {
        if (node.Value.Value is IDisposable disposable)
            disposable.Dispose();
    }
}
