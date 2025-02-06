﻿using LruCache.Abstractions;

namespace LruCache;

public partial class LruCache<TKey, TValue> : ILruCache<TKey, TValue>, ICacheMetrics, ICacheUtils<TKey, TValue>, IDisposable where TKey : notnull
{
    private long _hits;
    private long _misses;
    private long _evictions;

    public int Capacity => _capacity;

    public int Count
    {
        get { lock (_syncLock) { return _cacheMap.Count; } }
    }

    public long Hits => Interlocked.Read(ref _hits);
    public long Misses => Interlocked.Read(ref _misses);
    public long Evictions => Interlocked.Read(ref _evictions);
}
