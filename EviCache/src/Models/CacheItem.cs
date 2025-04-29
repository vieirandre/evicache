using EviCache.Enums;
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
        if (expiration is null)
            return;

        Metadata.Expiration = expiration;

        if (expiration.TimeToLive.HasValue && expiration.Mode == ExpirationMode.Absolute)
            Metadata.ExpiresAt = DateTimeOffset.UtcNow.Add(expiration.TimeToLive.Value);
        else
            Metadata.ExpiresAt = null;
    }
}
