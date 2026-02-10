using EviCache.Abstractions;
using EviCache.Extensions;

namespace EviCache;

public sealed partial class Cache<TKey, TValue> : ICacheMetrics where TKey : notnull
{
    private long _hits;
    private long _misses;
    private long _evictions;

    public int Capacity => _capacity;

    public int Count
    {
        get
        {
            using var _ = _gate.Lock();
            PurgeExpiredItems();

            return _cacheMap.Count;
        }
    }

    public long Hits => Interlocked.Read(ref _hits);
    public long Misses => Interlocked.Read(ref _misses);
    public long Evictions => Interlocked.Read(ref _evictions);

    public double HitRate
    {
        get
        {
            var h = Hits;
            var total = h + Misses;
            return total == 0 ? 0 : (double)h / total;
        }
    }
}
