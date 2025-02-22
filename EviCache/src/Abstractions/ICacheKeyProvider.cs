using System.Collections.Immutable;

namespace EviCache.Abstractions;

public interface ICacheKeyProvider<TKey> where TKey : notnull
{
    ImmutableList<TKey> GetKeys();
}
