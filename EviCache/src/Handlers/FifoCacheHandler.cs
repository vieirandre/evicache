using EviCache.Abstractions;
using System.Collections.Immutable;

namespace EviCache.Handlers;

internal class FifoCacheHandler<TKey> : ICacheHandler<TKey>, IEvictionCandidateSelector<TKey> where TKey : notnull
{
    private readonly LinkedList<TKey> _fifoList = new();

    public void RegisterAccess(TKey key) { }

    public void RegisterInsertion(TKey key) => _fifoList.AddLast(key);

    public void RegisterUpdate(TKey key) { }

    public void RegisterRemoval(TKey key) => _fifoList.Remove(key);

    public void Clear() => _fifoList.Clear();

    public bool TrySelectEvictionCandidate(out TKey candidate)
    {
        if (_fifoList.First is null)
        {
            candidate = default!;
            return false;
        }

        candidate = _fifoList.First.Value;
        return true;
    }

    public ImmutableList<TKey> GetKeys() => _fifoList.ToImmutableList();
}