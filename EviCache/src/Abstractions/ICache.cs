﻿namespace EviCache.Abstractions;

public interface ICache<TKey, TValue> :
    ICacheOperations<TKey, TValue>,
    ICacheOperationsAsync<TKey, TValue>,
    ICacheMetrics,
    ICacheInspection<TKey, TValue>,
    ICacheMetadata<TKey>,
    IDisposable
    where TKey : notnull
{
}