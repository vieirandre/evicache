using EviCache.Abstractions;
using Microsoft.Extensions.Logging;

namespace EviCache;

public partial class Cache<TKey, TValue> : ICacheOperations<TKey, TValue>, IDisposable where TKey : notnull
{
    public void Clear()
    {
        int removedCount;
        List<IDisposable> disposables;

        lock (_syncLock)
        {
            removedCount = _cacheMap.Count;

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
                    _logger.LogError(ex, "Error while disposing cache item in the background");
                }
            }
        });

        _logger.LogInformation("Cache cleared. Removed {Count} items", removedCount);
    }

    public void Dispose() => Clear();

    private static void DisposeItem(TValue? value)
    {
        if (value is IDisposable disposable)
            disposable.Dispose();
    }
}
