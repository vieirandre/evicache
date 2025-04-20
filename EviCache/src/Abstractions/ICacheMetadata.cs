using EviCache.Models;

namespace EviCache.Abstractions;

public interface ICacheMetadata<TKey> where TKey : notnull
{
    CacheItemMetadata GetMetadata(TKey key);
    bool TryGetMetadata(TKey key, out CacheItemMetadata? metadata);
}
