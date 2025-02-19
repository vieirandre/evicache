using EviCache.Enums;

namespace EviCache.Options;

public record CacheOptions(int Capacity, EvictionPolicy EvictionPolicy);
