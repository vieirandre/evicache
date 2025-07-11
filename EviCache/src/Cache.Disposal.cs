﻿using EviCache.Models;

namespace EviCache;

public sealed partial class Cache<TKey, TValue> : IDisposable where TKey : notnull
{
    /// <summary>
    /// Releases all resources used by the cache, including clearing all items and disposing of any disposable values.
    /// </summary>
    public void Dispose()
    {
        Clear();
        _gate.Dispose();
    }

    private static void DisposeItem(CacheItem<TValue> cacheItem)
    {
        if (cacheItem.Value is IDisposable d)
            d.Dispose();
        else if (cacheItem.Value is IAsyncDisposable ad)
            ad.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    private static async Task DisposeValueAsync(object value)
    {
        if (value is IAsyncDisposable ad)
            await ad.DisposeAsync().ConfigureAwait(false);
        else if (value is IDisposable d)
            d.Dispose();
    }
}
