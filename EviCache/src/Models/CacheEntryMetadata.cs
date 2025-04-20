namespace EviCache.Models;

internal class CacheEntryMetadata
{
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset LastAccessedAt { get; private set; }
    public DateTimeOffset LastUpdatedAt { get; private set; }
}
