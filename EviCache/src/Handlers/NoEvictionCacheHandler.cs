using EviCache.Abstractions;
using System.Collections.Immutable;

namespace EviCache.Handlers;

internal class NoEvictionCacheHandler<TKey> : CacheHandlerBase<TKey> where TKey : notnull
{
    private readonly HashSet<TKey> _keys = new();

    public override void RegisterInsertion(TKey key) => _keys.Add(key);

    public override void RegisterRemoval(TKey key) => _keys.Remove(key);

    public override void Clear() => _keys.Clear();

    public override ImmutableList<TKey> GetKeys() => _keys.ToImmutableList();
}