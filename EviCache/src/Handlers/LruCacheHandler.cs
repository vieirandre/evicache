using EviCache.Abstractions;
using System.Collections.Immutable;

namespace EviCache.Handlers;

internal class LruCacheHandler<TKey> : ICacheHandler<TKey> where TKey : notnull
{
    private readonly LinkedList<TKey> _lruList = new();

    public void RegisterAccess(TKey key)
    {
        _lruList.Remove(key);
        _lruList.AddFirst(key);
    }

    public void RegisterInsertion(TKey key)
    {
        _lruList.AddFirst(key);
    }

    public void RegisterUpdate(TKey key)
    {
        RegisterAccess(key);
    }

    public void RegisterRemoval(TKey key)
    {
        _lruList.Remove(key);
    }

    public void Clear()
    {
        _lruList.Clear();
    }

    public bool TrySelectEvictionCandidate(out TKey candidate)
    {
        if (_lruList.Last is null)
        {
            candidate = default!;
            return false;
        }

        candidate = _lruList.Last.Value;
        return true;
    }

    public ImmutableList<TKey> GetKeys()
    {
        return _lruList.ToImmutableList();
    }
}