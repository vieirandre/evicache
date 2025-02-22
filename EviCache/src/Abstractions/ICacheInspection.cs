using System.Collections.Immutable;

namespace EviCache.Abstractions;

public interface ICacheInspection<TKey, TValue> : ICacheKeyProvider<TKey> where TKey : notnull
{
    ImmutableList<KeyValuePair<TKey, TValue>> GetSnapshot();
}
