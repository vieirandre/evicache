using EviCache.Abstractions;
using System.Collections.Immutable;

namespace EviCache.Handlers;

public class LfuCacheHandler<TKey, TValue> : ICacheHandler<TKey, TValue> where TKey : notnull
{
    public ImmutableList<TKey> InternalCollection => throw new NotImplementedException();

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public void RecordAccess(TKey key)
    {
        throw new NotImplementedException();
    }

    public void RecordInsertion(TKey key)
    {
        throw new NotImplementedException();
    }

    public void RecordRemoval(TKey key)
    {
        throw new NotImplementedException();
    }

    public void RecordUpdate(TKey key)
    {
        throw new NotImplementedException();
    }

    public bool TrySelectEvictionCandidate(out TKey candidate)
    {
        throw new NotImplementedException();
    }
}
