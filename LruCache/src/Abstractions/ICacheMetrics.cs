namespace LruCache.Abstractions;

public interface ICacheMetrics
{
    int Capacity { get; }
    int Count { get; }
    long Hits { get; }
    long Misses { get; }
    long Evictions { get; }
}
