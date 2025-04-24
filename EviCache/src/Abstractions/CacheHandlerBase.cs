using System.Collections.Immutable;

namespace EviCache.Abstractions;

internal abstract class CacheHandlerBase<TKey> where TKey : notnull
{
    public virtual void RegisterAccess(TKey key) { }

    public virtual void RegisterInsertion(TKey key) { }

    public virtual void RegisterUpdate(TKey key) { }

    public virtual void RegisterRemoval(TKey key) { }

    public virtual void Clear() { }

    public abstract ImmutableList<TKey> GetKeys();
}
