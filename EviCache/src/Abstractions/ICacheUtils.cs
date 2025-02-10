using System.Collections.Immutable;

namespace EviCache.Abstractions;

public interface ICacheUtils<TKey, TValue> where TKey : notnull
{
    ImmutableList<TKey> GetKeysInOrder();
    ImmutableList<KeyValuePair<TKey, TValue>> GetSnapshot();
}
