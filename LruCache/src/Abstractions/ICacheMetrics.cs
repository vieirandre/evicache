namespace LruCache.Abstractions;

public interface ICacheMetrics
{
    int Capacity { get; }
    int Count { get; }
    long HitCount { get; }
    long MissCount { get; }
    long EvictionCount { get; }
}
