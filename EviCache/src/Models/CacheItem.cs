namespace EviCache.Models;

internal sealed class CacheItem<TValue> : IDisposable
{
    public TValue Value { get; private set; }
    public CacheItemMetadata Metadata { get; }

    public CacheItem(TValue value)
    {
        Value = value;
        Metadata = new CacheItemMetadata();
    }

    public CacheItem(TValue value, TimeSpan ttl)
    {
        Value = value;
        Metadata = new CacheItemMetadata();

        SetTtl(ttl);
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

    internal void UpdateTtl(TimeSpan ttl)
    {
        SetTtl(ttl);
        Metadata.RegisterUpdate();
    }

    private void SetTtl(TimeSpan ttl) => Metadata.ExpiresAt = DateTimeOffset.UtcNow.Add(ttl);
}
