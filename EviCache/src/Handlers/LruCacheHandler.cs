using EviCache.Abstractions;
using System.Collections.Immutable;

namespace EviCache.Handlers;

internal class LruCacheHandler<TKey> : CacheHandlerBase<TKey>, IEvictionCandidateSelector<TKey> where TKey : notnull
{
    private readonly LinkedList<TKey> _lruList = new();

    public override void RegisterAccess(TKey key)
    {
        _lruList.Remove(key);
        _lruList.AddFirst(key);
    }

    public override void RegisterInsertion(TKey key) => _lruList.AddFirst(key);

    public override void RegisterUpdate(TKey key) => RegisterAccess(key);

    public override void RegisterRemoval(TKey key) => _lruList.Remove(key);

    public override void Clear() => _lruList.Clear();

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

    public override ImmutableList<TKey> GetKeys() => _lruList.ToImmutableList();
}