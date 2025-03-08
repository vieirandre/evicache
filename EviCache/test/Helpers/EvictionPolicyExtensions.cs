using EviCache.Enums;

namespace EviCache.Tests.Helpers;

public static class EvictionPolicyExtensions
{
    public static string GetEvictionCandidateCollectionFieldName(this EvictionPolicy policy)
    {
        return policy switch
        {
            EvictionPolicy.LRU => "_lruList",
            EvictionPolicy.FIFO => "_fifoList",
            EvictionPolicy.LFU => "_frequencyBuckets",
            _ => throw new NotSupportedException("N/A")
        };
    }
}
