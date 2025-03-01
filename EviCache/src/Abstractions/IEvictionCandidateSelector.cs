namespace EviCache.Abstractions;

internal interface IEvictionCandidateSelector<TKey> where TKey : notnull
{
    bool TrySelectEvictionCandidate(out TKey candidate);
}
