namespace EviCache.Models;

internal sealed class CacheItem<TValue> : IDisposable
{
    public TValue Value { get; }
    public CacheItemMetadata Metadata { get; }

    public CacheItem(TValue value)
    {
        Value = value;
        Metadata = new CacheItemMetadata();
    }

    public void Dispose()
    {
        if (Value is IDisposable disposable)
            disposable.Dispose();
    }
}
