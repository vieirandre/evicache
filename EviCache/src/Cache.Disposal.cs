﻿using EviCache.Abstractions;

namespace EviCache;

public partial class Cache<TKey, TValue> : ICacheOperations<TKey, TValue>, ICacheMetrics, ICacheInspection<TKey, TValue>, IDisposable where TKey : notnull
{
    public void Clear()
    {
        List<IDisposable> disposables;

        lock (_syncLock)
        {
            disposables = _cacheMap.Values
                .OfType<IDisposable>()
                .ToList();

            _cacheMap.Clear();
            _cacheHandler.Clear();
        }

        _ = Task.Run(() =>
        {
            foreach (var disposable in disposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while disposing cache item in the background: {ex}");
                }
            }
        });
    }

    public void Dispose() => Clear();

    private static void DisposeItem(TValue? value)
    {
        if (value is IDisposable disposable)
            disposable.Dispose();
    }
}
