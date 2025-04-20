namespace EviCache.Models;

public class CacheEntryMetadata
{
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset LastAccessedAt { get; private set; }
    public DateTimeOffset LastUpdatedAt { get; private set; }
    public long AccessCount => Interlocked.Read(ref _accessCount);

    private long _accessCount;

    internal CacheEntryMetadata()
    {
        var now = DateTimeOffset.UtcNow;
        CreatedAt = now;
        LastAccessedAt = now;
        LastUpdatedAt = now;
        _accessCount = 0;
    }

    internal void RegisterAccess()
    {
        LastAccessedAt = DateTimeOffset.UtcNow;
        Interlocked.Increment(ref _accessCount);
    }

    internal void RegisterUpdate()
    {
        LastUpdatedAt = DateTimeOffset.UtcNow;
    }
}
