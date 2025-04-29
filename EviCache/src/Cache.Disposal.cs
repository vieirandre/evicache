using EviCache.Models;

namespace EviCache;

public sealed partial class Cache<TKey, TValue> : IDisposable where TKey : notnull
{
    /// <summary>
    /// Releases all resources used by the cache, including clearing all items and disposing of any disposable values.
    /// </summary>
    public void Dispose() => Clear();

    private static void DisposeItem(CacheItem<TValue>? value)
    {
        if (value is IDisposable disposable)
            disposable.Dispose();
    }
}
