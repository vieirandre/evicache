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
