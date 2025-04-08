using EviCache.Abstractions;
using System.Collections.Immutable;

namespace EviCache.Handlers;

internal class NoEvictionCacheHandler<TKey> : ICacheHandler<TKey> where TKey : notnull
{
    private readonly HashSet<TKey> _keys = new();

    public void RegisterAccess(TKey key) { }

    public void RegisterInsertion(TKey key) => _keys.Add(key);

    public void RegisterUpdate(TKey key) { }

    public void RegisterRemoval(TKey key) => _keys.Remove(key);

    public void Clear() => _keys.Clear();

    public ImmutableList<TKey> GetKeys() => _keys.ToImmutableList();
}