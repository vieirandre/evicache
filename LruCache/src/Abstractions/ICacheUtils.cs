using System.Collections.Immutable;

namespace LruCache.Abstractions;

public interface ICacheUtils<TKey>
{
    ImmutableList<TKey> GetKeysInOrder();
}
