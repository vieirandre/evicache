using EviCache.Models;

namespace EviCache;

public sealed partial class Cache<TKey, TValue> : IDisposable where TKey : notnull
{
    public void Dispose() => Clear();

    private static void DisposeItem(CacheItem<TValue>? value)
    {
        if (value is IDisposable disposable)
            disposable.Dispose();
    }
}
