namespace EviCache.Models;

internal class CacheEntry<TValue>
{
    public TValue Value { get; }
    public CacheEntryMetadata Metadata { get; }

    public CacheEntry(TValue value)
    {
        Value = value;
        Metadata = new CacheEntryMetadata();
    }
}
