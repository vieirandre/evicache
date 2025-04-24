namespace EviCache.Models;

internal sealed class CacheItem<TValue> : IDisposable
{
    public TValue Value { get; private set; }
    public CacheItemMetadata Metadata { get; }

    public CacheItem(TValue value, TimeSpan? ttl = null)
    {
        Value = value;
        Metadata = new CacheItemMetadata();

        if (ttl.HasValue)
            Metadata.ExpiresAt = DateTimeOffset.UtcNow.Add(ttl.Value);
    }

    public void Dispose()
    {
        if (Value is IDisposable disposable)
            disposable.Dispose();
    }

    internal void ReplaceValue(TValue newValue)
    {
        if (!ReferenceEquals(Value, newValue) && Value is IDisposable d)
            d.Dispose();

        Value = newValue;
        Metadata.RegisterUpdate();
    }
}
