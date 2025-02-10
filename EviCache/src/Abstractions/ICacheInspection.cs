using System.Collections.Immutable;

namespace EviCache.Abstractions;

public interface ICacheInspection<TKey> where TKey : notnull
{
    ImmutableList<TKey> GetKeysInOrder();
}
