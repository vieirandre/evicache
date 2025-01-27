using LruCache.Models;

namespace LruCache;

public class LruCache<TKey, TValue> : ILruCache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, LinkedListNode<CacheItem<TKey, TValue>>> _cacheMap;
    private readonly LinkedList<CacheItem<TKey, TValue>> _lruList;

    public LruCache(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

        _capacity = capacity;
        _cacheMap = new Dictionary<TKey, LinkedListNode<CacheItem<TKey, TValue>>>(capacity);
        _lruList = new LinkedList<CacheItem<TKey, TValue>>();
    }

    public bool TryGet(TKey key, out TValue value)
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

    public void Put(TKey key, TValue value)
    {
        if (_cacheMap.TryGetValue(key, out var existingNode))
        {
            existingNode.Value.Value = value;

            MoveToFront(existingNode);

            return;
        }

        var newItem = new CacheItem<TKey, TValue>(key, value);
        var newNode = new LinkedListNode<CacheItem<TKey, TValue>>(newItem);

        _lruList.AddFirst(newNode);
        _cacheMap[key] = newNode;
    }

    private void MoveToFront(LinkedListNode<CacheItem<TKey, TValue>> node)
    {
        _lruList.Remove(node);
        _lruList.AddFirst(node);
    }
}
