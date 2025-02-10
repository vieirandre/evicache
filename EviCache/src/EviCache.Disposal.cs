using EviCache.Abstractions;
using EviCache.Models;

namespace EviCache;

public partial class EviCache<TKey, TValue> : ILruCache<TKey, TValue>, ICacheMetrics, ICacheUtils<TKey, TValue>, IDisposable where TKey : notnull
{
    public void Clear()
    {
        List<IDisposable> disposables;

        lock (_syncLock)
        {
            disposables = _cacheMap.Values
                .Select(node => node.Value)
                .OfType<IDisposable>()
                .ToList();

            _cacheMap.Clear();
            _lruList.Clear();
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

    private static void DisposeItem(LinkedListNode<CacheItem<TKey, TValue>> node)
    {
        if (node.Value.Value is IDisposable disposable)
            disposable.Dispose();
    }
}
