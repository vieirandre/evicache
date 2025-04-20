using EviCache.Models;

namespace EviCache.Abstractions;

public interface ICacheMetadataOperations<TKey> where TKey : notnull
{
    CacheEntryMetadata GetMetadata(TKey key);
    bool TryGetMetadata(TKey key, out CacheEntryMetadata? metadata);
}
