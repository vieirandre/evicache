using System.Collections.Immutable;

namespace EviCache.Abstractions;

public interface ICacheHandler<TKey, TValue> where TKey : notnull
{
    ImmutableList<TKey> InternalCollection { get; }
    void RecordAccess(TKey key);
    void RecordInsertion(TKey key);
    void RecordUpdate(TKey key);
    void RecordRemoval(TKey key);
    void Clear();
    bool TrySelectEvictionCandidate(out TKey candidate);
}