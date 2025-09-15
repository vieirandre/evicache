using EviCache.Options;

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

    public CacheItem(TValue value, CacheItemOptions options)
    {
        Value = value;
        Metadata = new CacheItemMetadata();

        SetExpiration(options.Expiration);
    }

    public void Dispose()
    {
        if (Value is IDisposable disposable)
            disposable.Dispose();
    }

    internal void UpdateItem(TValue newValue)
    {
        if (!ReferenceEquals(Value, newValue) && Value is IDisposable d)
            d.Dispose();

        Value = newValue;

        if (Metadata.Expiration is ExpirationOptions.Absolute abs)
            SetExpiration(abs);

        Metadata.RegisterUpdate();
        Metadata.RegisterAccess();
    }

    internal void UpdateItem(TValue newValue, CacheItemOptions options)
    {
        if (!ReferenceEquals(Value, newValue) && Value is IDisposable d)
            d.Dispose();

        Value = newValue;
        SetExpiration(options.Expiration);

        Metadata.RegisterUpdate();
        Metadata.RegisterAccess();
    }

    private void SetExpiration(ExpirationOptions? expiration)
    {
        Metadata.Expiration = expiration;

        Metadata.ExpiresAt = expiration switch
        {
            ExpirationOptions.Absolute abs => DateTimeOffset.UtcNow + abs.TimeToLive,
            _ => null
        };
    }
}
