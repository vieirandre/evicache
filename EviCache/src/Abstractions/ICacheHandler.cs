using System.Collections.Immutable;

namespace EviCache.Abstractions;

internal interface ICacheHandler<TKey, TValue> : ICacheKeyProvider<TKey> where TKey : notnull
{
    void RegisterAccess(TKey key);
    void RegisterInsertion(TKey key);
    void RegisterUpdate(TKey key);
    void RegisterRemoval(TKey key);
    void Clear();
    bool TrySelectEvictionCandidate(out TKey candidate);
}