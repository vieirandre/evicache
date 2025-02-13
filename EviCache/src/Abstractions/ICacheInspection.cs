using System.Collections.Immutable;

namespace EviCache.Abstractions;

public interface ICacheInspection<TKey, TValue> where TKey : notnull
{
    ImmutableList<TKey> GetKeys();
    ImmutableList<KeyValuePair<TKey, TValue>> GetSnapshot();
}
