using EviCache.Abstractions;
using System.Collections.Immutable;

namespace EviCache.Handlers;

internal class FifoCacheHandler<TKey> : CacheHandlerBase<TKey>, IEvictionCandidateSelector<TKey> where TKey : notnull
{
    private readonly LinkedList<TKey> _fifoList = new();

    public override void RegisterInsertion(TKey key) => _fifoList.AddLast(key);
    public override void RegisterRemoval(TKey key) => _fifoList.Remove(key);
    public override void Clear() => _fifoList.Clear();

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

    public override ImmutableList<TKey> GetKeys() => _fifoList.ToImmutableList();
}