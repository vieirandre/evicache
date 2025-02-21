using EviCache.Abstractions;
using System.Collections.Immutable;

namespace EviCache.Handlers;

public class LruCacheHandler<TKey, TValue> : ICacheHandler<TKey, TValue> where TKey : notnull
{
    private readonly LinkedList<TKey> _cacheList = new();

    public ImmutableList<TKey> InternalCollection => _cacheList.ToImmutableList();

    public void RegisterAccess(TKey key)
    {
        _cacheList.Remove(key);
        _cacheList.AddFirst(key);
    }

    public void RegisterInsertion(TKey key)
    {
        _cacheList.AddFirst(key);
    }

    public void RegisterUpdate(TKey key)
    {
        RegisterAccess(key);
    }

    public void RegisterRemoval(TKey key)
    {
        _cacheList.Remove(key);
    }

    public void Clear()
    {
        _cacheList.Clear();
    }

    public bool TrySelectEvictionCandidate(out TKey candidate)
    {
        if (_cacheList.Last is null)
        {
            candidate = default!;
            return false;
        }

        candidate = _cacheList.Last.Value;
        return true;
    }
}