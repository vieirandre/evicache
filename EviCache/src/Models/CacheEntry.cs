namespace EviCache.Models;

internal sealed class CacheEntry<TValue> : IDisposable
{
    public TValue Value { get; }
    public CacheEntryMetadata Metadata { get; }

    public CacheEntry(TValue value)
    {
        Value = value;
        Metadata = new CacheEntryMetadata();
    }

    public void Dispose()
    {
        if (Value is IDisposable disposable)
            disposable.Dispose();
    }
}
