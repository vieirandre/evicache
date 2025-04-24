using EviCache.Enums;
using EviCache.Exceptions;
using EviCache.Options;
using EviCache.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Immutable;

namespace EviCache.Tests;

public abstract class CacheTestsBase
{
    protected abstract EvictionPolicy EvictionPolicy { get; }
    protected virtual bool SupportsEviction => true;
    protected readonly Mock<ILogger> _loggerMock;

    protected Cache<TKey, TValue> CreateCache<TKey, TValue>(int capacity) where TKey : notnull
    {
        var options = new CacheOptions(capacity, EvictionPolicy);
        return new Cache<TKey, TValue>(options);
    }

    protected Cache<TKey, TValue> CreateCache<TKey, TValue>(int capacity, ILogger logger) where TKey : notnull
    {
        var options = new CacheOptions(capacity, EvictionPolicy);
        return new Cache<TKey, TValue>(options, logger);
    }

    public CacheTestsBase()
    {
        _loggerMock = new Mock<ILogger>();

        _loggerMock
            .Setup(logger => logger.IsEnabled(LogLevel.Debug))
            .Returns(true);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Should_ThrowArgumentOutOfRangeException_WhenCapacityIsZeroOrNegative(int invalidCapacity)
    {
        // act & assert

        Assert.Throws<ArgumentOutOfRangeException>(() => CreateCache<int, string>(invalidCapacity));
    }

    [Fact]
    public void Should_LogInitializationInformation()
    {
        // arrange

        int capacity = 3;

        // act

        _ = CreateCache<int, string>(capacity, _loggerMock.Object);

        // assert

        _loggerMock.VerifyLog(LogLevel.Information, $"Cache initialized with capacity {capacity} and eviction policy {EvictionPolicy}", Times.Once());
    }

    [Fact]
    public void Should_LogCacheClearInformation()
    {
        // arrange

        int capacity = 2;
        var cache = CreateCache<int, string>(capacity, _loggerMock.Object);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        cache.Clear();

        // assert

        _loggerMock.VerifyLog(LogLevel.Information, $"Cache cleared. Removed {capacity} items", Times.Once());
    }

    [Fact]
    public void Should_ReturnValue_WhenKeyIsInsertedAndRetrieved()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        // act

        cache.Put(1, "value");
        bool found = cache.TryGet(1, out var result);

        // assert

        Assert.True(found);
        Assert.Equal("value", result);
    }

    [Fact]
    public void Should_StoreAndRetrieve_WhenKeyIsStringAndValueIsInt()
    {
        // arrange

        var cache = CreateCache<string, int>(2);

        // act

        cache.Put("this", 10);
        cache.Put("that", 20);
        bool found = cache.TryGet("that", out var thatValue);

        // arrange

        Assert.True(found);
        Assert.Equal(20, thatValue);
    }

    [SkippableFact]
    public void Should_UpdateExistingKeyAndEvict_WhenReinsertingKeyOverCapacity()
    {
        Skip.IfNot(SupportsEviction, TestMessages.EvictionNotSupported);
        Skip.If(EvictionPolicy.Equals(EvictionPolicy.FIFO), "FIFO does not reorder on updates");

        // arrange

        var cache = CreateCache<int, string>(2);
        cache.Put(1, "oldValue");
        cache.Put(2, "value2");

        // act

        cache.Put(1, "newValue");
        cache.Put(3, "value3");

        // assert

        Assert.False(cache.TryGet(2, out _));
        Assert.Equal("newValue", cache.Get(1));
        Assert.Equal("value3", cache.Get(3));
    }

    [Fact]
    public void Should_NotDisposeItem_WhenTryGetIsCalled()
    {
        // arrange

        var cache = CreateCache<int, DisposableDummy>(2);

        var disposableItem = new DisposableDummy();
        cache.Put(1, disposableItem);

        // act

        bool found = cache.TryGet(1, out var result);

        // assert

        Assert.True(found);
        Assert.False(disposableItem.IsDisposed);
        Assert.Equal(disposableItem, result);
    }

    [SkippableFact]
    public void Should_NotExceedCapacity_WhenCacheIsFull()
    {
        Skip.IfNot(SupportsEviction, TestMessages.EvictionNotSupported);

        // arrange

        int capacity = 2;
        var cache = CreateCache<int, string>(capacity);

        // act

        for (int i = 0; i < capacity * 2; i++)
        {
            cache.Put(i, $"value{i}");
        }

        // assert

        Assert.True(cache.Count <= capacity);
    }

    [Fact]
    public void Should_RemoveKeyAndDisposeItem_WhenKeyExists()
    {
        // arrange

        var cache = CreateCache<int, DisposableDummy>(2);

        var disposableItem = new DisposableDummy();
        cache.Put(1, disposableItem);
        cache.Put(2, new DisposableDummy());

        // act

        bool removed = cache.Remove(1);

        // assert

        Assert.True(removed);
        Assert.Equal(1, cache.Count);
        Assert.True(disposableItem.IsDisposed);
        Assert.False(cache.TryGet(1, out _));
    }

    [Fact]
    public void Should_ReturnFalseAndNotDispose_WhenKeyDoesNotExist()
    {
        // arrange

        var cache = CreateCache<int, DisposableDummy>(2);

        var disposableItem = new DisposableDummy();
        cache.Put(1, disposableItem);

        // aact

        bool removed = cache.Remove(9);

        // assert

        Assert.False(removed);
        Assert.False(disposableItem.IsDisposed);
    }

    [Fact]
    public void Should_ClearAllDisposableItems()
    {
        // arrange

        var cache = CreateCache<int, string>(3, _loggerMock.Object);

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        cache.Clear();

        // assert

        Assert.Equal(0, cache.Count);
        Assert.False(cache.TryGet(1, out _));
        Assert.False(cache.TryGet(2, out _));
        Assert.False(cache.TryGet(3, out _));

        _loggerMock.VerifyLog(LogLevel.Information, $"Cache cleared. Removed 3 items", Times.Once());
    }

    [SkippableFact]
    public void Should_TrackHitsMissesAndEvictions_WhenCacheCapacityIs15()
    {
        Skip.IfNot(SupportsEviction, TestMessages.EvictionNotSupported);

        // arrange

        int capacity = 15;
        var cache = CreateCache<int, string>(capacity);

        // act & assert

        for (int i = 1; i <= capacity; i++)
        {
            cache.Put(i, $"value{i}");
        }

        for (int i = 1; i <= 10; i++)
        {
            string value = cache.Get(i);
            Assert.Equal($"value{i}", value);
        }

        for (int i = 16; i <= 20; i++)
        {
            bool found = cache.TryGet(i, out _);
            Assert.False(found);
        }

        for (int i = 16; i <= 20; i++)
        {
            cache.Put(i, $"value{i}");
        }

        Assert.Equal(capacity, cache.Count);
        Assert.Equal(10, cache.Hits);
        Assert.Equal(5, cache.Misses);
        Assert.Equal(5, cache.Evictions);
    }

    [Fact]
    public void Should_AddNewValue_WhenKeyDoesNotExistInGetOrAdd()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        // act

        var result = cache.GetOrAdd(1, "value1");

        // assert

        Assert.Equal("value1", result);
        Assert.True(cache.TryGet(1, out var storedValue));
        Assert.Equal("value1", storedValue);
    }

    [Fact]
    public void Should_ReturnExistingValue_WhenKeyExistsInGetOrAdd()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        var result = cache.GetOrAdd(1, "newValue1");

        // assert

        Assert.Equal("value1", result);
    }

    [SkippableFact]
    public void Should_Evict_WhenAddingNewItemExceedsCapacityInGetOrAdd()
    {
        Skip.IfNot(SupportsEviction, TestMessages.EvictionNotSupported);

        // arrange

        var cache = CreateCache<int, string>(2);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        var result = cache.GetOrAdd(3, "value3");

        // assert

        Assert.Equal("value3", result);
        Assert.False(cache.TryGet(1, out _));
        Assert.True(cache.TryGet(2, out var value2));
        Assert.Equal("value2", value2);
        Assert.True(cache.TryGet(3, out var value3));
        Assert.Equal("value3", value3);
    }

    [Fact]
    public void Should_AddNewValueAndNotEvict_WhenUnderCapacityInGetOrAdd()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        var result = cache.GetOrAdd(3, "value3");

        // assert

        Assert.Equal("value3", result);
        Assert.Equal(3, cache.Count);
        Assert.True(cache.TryGet(1, out var value1));
        Assert.Equal("value1", value1);
        Assert.True(cache.TryGet(2, out var value2));
        Assert.Equal("value2", value2);
        Assert.True(cache.TryGet(3, out var value3));
        Assert.Equal("value3", value3);
    }

    [Fact]
    public void Should_AddAndReturnNewValue_WhenUsingGetOrAddWithDisposableValue()
    {
        // arrange

        var cache = CreateCache<int, DisposableDummy>(2);

        // act

        var disposable1 = cache.GetOrAdd(1, new DisposableDummy());

        // assert

        Assert.NotNull(disposable1);
        Assert.False(disposable1.IsDisposed);
        Assert.True(cache.TryGet(1, out var retrieved));
        Assert.Equal(disposable1, retrieved);
    }

    [Fact]
    public void Should_ReturnValue_WhenKeyExists()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        var result = cache.Get(2);

        // assert

        Assert.Equal("value2", result);
    }

    [Fact]
    public void Should_ThrowKeyNotFoundException_WhenKeyDoesNotExist()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        var exception = Assert.Throws<KeyNotFoundException>(() => cache.Get(3));

        // assert

        Assert.Equal("The key '3' was not found in the cache", exception.Message);
    }

    [Fact]
    public void Should_NotDisposeItem_WhenGetIsCalled()
    {
        // arrange

        var cache = CreateCache<int, DisposableDummy>(2);

        var disposableItem = new DisposableDummy();
        cache.Put(1, disposableItem);

        // act

        var result = cache.Get(1);

        // assert

        Assert.False(disposableItem.IsDisposed);
        Assert.Equal(disposableItem, result);
    }

    [Fact]
    public void Should_UpdateExistingKey_AndReturnNewValue()
    {
        // arrange

        var cache = CreateCache<int, string>(5);

        cache.Put(1, "oldValue");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        var result = cache.AddOrUpdate(1, "newValue");
        cache.Get(3);

        // assert

        Assert.Equal("newValue", result);
        Assert.Equal("newValue", cache.Get(1));
    }

    [Fact]
    public void Should_AddNewKey_AndReturnValue()
    {
        // arrange

        var cache = CreateCache<int, string>(5);

        // act

        var result = cache.AddOrUpdate(2, "value2");

        // assert

        Assert.Equal("value2", result);
        Assert.Equal("value2", cache.Get(2));
        Assert.Equal(1, cache.Count);
    }

    [SkippableFact]
    public void Should_Evict_WhenAddingNewKeyExceedingCapacity()
    {
        Skip.IfNot(SupportsEviction, TestMessages.EvictionNotSupported);

        // arrange

        var cache = CreateCache<int, string>(3);

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");
        cache.Get(2);

        // act

        var result = cache.AddOrUpdate(4, "value4");

        // assert

        Assert.Equal("value4", result);
        Assert.Equal(3, cache.Count);
        Assert.False(cache.TryGet(1, out _));
        Assert.Equal(1, cache.Evictions);
    }

    [Fact]
    public void Should_UpdateMetrics_OnAddOrUpdate_ForExistingAndNewKeys()
    {
        // arrange

        var cache = CreateCache<int, string>(5);

        // act & assert

        cache.AddOrUpdate(1, "value1");
        Assert.Equal(1, cache.Misses);
        Assert.Equal(0, cache.Hits);

        cache.AddOrUpdate(1, "value1Updated");
        Assert.Equal(1, cache.Hits);

        cache.AddOrUpdate(2, "value2");
        Assert.Equal(2, cache.Misses);
    }

    [Fact]
    public void ContainsKey_ReturnsTrue_WhenKeyExists()
    {
        // arrange

        var cache = CreateCache<int, string>(3);
        cache.Put(1, "value1");

        // act

        bool contains = cache.ContainsKey(1);

        // assert

        Assert.True(contains);
    }

    [Fact]
    public void ContainsKey_ReturnsFalse_WhenKeyDoesNotExist()
    {
        // arrange

        var cache = CreateCache<int, string>(3);
        cache.Put(1, "value1");

        // act

        bool contains = cache.ContainsKey(2);

        // assert

        Assert.False(contains);
    }

    [Fact]
    public void ContainsKey_ReturnsFalse_AfterKeyIsRemoved()
    {
        // arrange

        var cache = CreateCache<int, string>(3);
        cache.Put(1, "value1");

        // act

        bool removed = cache.Remove(1);
        bool contains = cache.ContainsKey(1);

        // assert

        Assert.True(removed);
        Assert.False(contains);
    }

    [Fact]
    public void ContainsKey_ReturnsFalse_WhenCacheIsCleared()
    {
        // arrange

        var cache = CreateCache<int, string>(3, _loggerMock.Object);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        cache.Clear();
        bool containsKey1 = cache.ContainsKey(1);
        bool containsKey2 = cache.ContainsKey(2);

        // assert

        Assert.False(containsKey1);
        Assert.False(containsKey2);

        _loggerMock.VerifyLog(LogLevel.Information, $"Cache cleared. Removed 2 items", Times.Once());
    }

    [Fact]
    public void Should_HandleCapacityOfOne()
    {
        // arrange

        var cache = CreateCache<int, string>(1, _loggerMock.Object);

        // act

        cache.Put(1, "value1");

        // assert

        Assert.True(cache.TryGet(1, out var value1));
        Assert.Equal("value1", value1);

        if (SupportsEviction)
        {
            // act

            cache.Put(2, "value2");

            // assert

            Assert.False(cache.TryGet(1, out _));
            Assert.True(cache.TryGet(2, out var value2));
            Assert.Equal("value2", value2);

            // act

            cache.Put(2, "newValue2");

            // assert

            Assert.True(cache.TryGet(2, out var updatedValue));
            Assert.Equal("newValue2", updatedValue);
        }
        else
        {
            // act

            var exception = Assert.Throws<CacheFullException>(() => cache.Put(2, "value2"));

            // assert

            Assert.True(cache.TryGet(1, out _));
            Assert.False(cache.TryGet(2, out _));

            // act

            cache.Put(1, "newValue1");

            // assert

            Assert.True(cache.TryGet(1, out var updatedValue));
            Assert.Equal("newValue1", updatedValue);
            Assert.Equal($"Cache is full (capacity: 1) and uses {EvictionPolicy} policy", exception.Message);
        }

        Assert.Equal(1, cache.Count);
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public async Task Should_HandleConcurrentAccess()
    {
        // arrange

        const int capacity = 50;
        const int taskCount = 50;
        const int operationsPerTask = 1000;

        CacheFullException putException = default!;
        CacheFullException addOrUpdateException = default!;
        CacheFullException getOrAddException = default!;

        var cache = CreateCache<int, int>(capacity, _loggerMock.Object);

        var tasks = new List<Task>();

        // act

        for (int t = 0; t < taskCount; t++)
        {
            tasks.Add(Task.Run(() =>
            {
                var localRandom = new Random(Guid.NewGuid().GetHashCode());

                for (int i = 0; i < operationsPerTask; i++)
                {
                    int key = localRandom.Next(1, 200);
                    int value = localRandom.Next();

                    int op = localRandom.Next(0, 5);
                    switch (op)
                    {
                        case 0:
                            try { cache.Put(key, value); }
                            catch (CacheFullException ex) { putException ??= ex; }
                            break;
                        case 1:
                            cache.TryGet(key, out _);
                            break;
                        case 2:
                            cache.Remove(key);
                            break;
                        case 3:
                            try { cache.AddOrUpdate(key, value); }
                            catch (CacheFullException ex) { addOrUpdateException ??= ex; }
                            break;
                        case 4:
                            try { cache.GetOrAdd(key, value); }
                            catch (CacheFullException ex) { getOrAddException ??= ex; }
                            break;
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);

        // assert

        Assert.True(cache.Count <= capacity);

        var keys = cache.GetKeys();

        foreach (var key in keys)
        {
            Assert.True(cache.TryGet(key, out _));
        }

        Assert.Equal(keys.Count, keys.Distinct().Count()); // no duplicates

        if (!SupportsEviction)
        {
            Assert.Equal($"Cache is full (capacity: {capacity}) and uses {EvictionPolicy} policy", putException?.Message);
            Assert.Equal($"Cache is full (capacity: {capacity}) and uses {EvictionPolicy} policy", addOrUpdateException?.Message);
            Assert.Equal($"Cache is full (capacity: {capacity}) and uses {EvictionPolicy} policy", getOrAddException?.Message);
        }

        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public async Task Should_HandleConcurrentUpdatesOnSameKey()
    {
        // arrange

        var cache = CreateCache<int, int>(10);

        int key = 25;
        cache.Put(key, 0);

        int taskCount = 50;
        int iterations = 1000;

        var tasks = new List<Task>();

        // act

        for (int t = 0; t < taskCount; t++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    cache.AddOrUpdate(key, i);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // assert

        Assert.Equal(1, cache.Count);
        Assert.Equal(0, cache.Evictions);
        Assert.True(cache.TryGet(key, out int finalValue));
        Assert.InRange(finalValue, 0, iterations - 1);
        Assert.Single(cache.GetKeys());
        Assert.Equal(taskCount * iterations + 1, cache.Hits); // '+1' due to tryGet used in assert
    }

    [Fact]
    public async Task Should_HandleRepeatedInsertionsAndRemovalsOnSameKey()
    {
        // arrange

        var cache = CreateCache<int, int>(10);

        int key = 1;
        int iterations = 10000;
        int taskCount = 10;

        var tasks = new List<Task>();

        // act

        for (int t = 0; t < taskCount; t++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    cache.Put(key, i);
                    cache.Remove(key);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // assert

        Assert.True(cache.Count <= 1);
        Assert.True(cache.GetKeys().Count <= 1);
    }

    [Fact]
    public void Should_ReturnConsistentKeysAndSnapshot()
    {
        // arrange

        int capacity = 15;
        var cache = CreateCache<int, string>(capacity);

        var numbers = Enumerable.Range(1, capacity);
        numbers.Shuffle();

        foreach (int n in numbers)
        {
            cache.Put(n, $"value{n}");
        }

        // act

        var keys = cache.GetKeys();
        var snapshotKeys = cache.GetSnapshot().Select(kvp => kvp.Key);

        // assert

        Assert.Equal(keys, snapshotKeys);
    }

    [Fact]
    public void Should_BeIdempotent_WhenClearingCacheMultipleTimes()
    {
        // arrange

        var cache = CreateCache<int, string>(3, _loggerMock.Object);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        cache.Clear();
        cache.Clear();

        // assert

        Assert.Equal(0, cache.Count);
        Assert.Empty(cache.GetKeys());

        _loggerMock.VerifyLog(LogLevel.Information, $"Cache cleared. Removed 2 items", Times.Once());
        _loggerMock.VerifyLog(LogLevel.Information, $"Cache cleared. Removed 0 items", Times.Once());
        _loggerMock.VerifyLog(LogLevel.Information, $"Cache cleared. Removed .* items", Times.Exactly(2));
    }

    [Fact]
    public void Should_UpdateMetricsAccurately_AfterMixedOperations()
    {
        // arrange

        var cache = CreateCache<int, string>(5);

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        cache.Get(1);
        cache.Get(2);

        // act & assert

        Assert.Throws<KeyNotFoundException>(() => cache.Get(4));

        // act

        cache.TryGet(4, out _);
        cache.AddOrUpdate(1, "newValue1");
        cache.Remove(2);
        cache.GetOrAdd(5, "value5");

        // assert

        Assert.Equal(3, cache.Count);
        Assert.Equal(3, cache.Hits);
        Assert.Equal(3, cache.Misses);
    }

    [Fact]
    public void Should_NotImpactMetrics_WhenContainsKeyIsCalled()
    {
        // arrange

        var cache = CreateCache<int, string>(3);
        cache.Put(1, "value1");

        // act

        bool containsExisting = cache.ContainsKey(1);
        bool containsMissing = cache.ContainsKey(99);

        // assert

        Assert.True(containsExisting);
        Assert.False(containsMissing);
        Assert.Equal(0, cache.Hits);
        Assert.Equal(0, cache.Misses);
    }

    [Fact]
    public async Task Should_YieldSameResults_ForSyncAndAsyncOperations() // TODO: expand
    {
        // arrange

        var syncCache = CreateCache<int, string>(5);
        var asyncCache = CreateCache<int, string>(5);

        // act (sync)

        syncCache.Put(1, "value1");
        syncCache.Put(2, "value2");
        string syncValue = syncCache.Get(1);

        // act (async)

        await asyncCache.PutAsync(1, "value1");
        await asyncCache.PutAsync(2, "value2");
        string asyncValue = await asyncCache.GetAsync(1);

        // assert

        Assert.Equal(syncCache.Count, asyncCache.Count);
        Assert.Equal(syncCache.GetKeys(), asyncCache.GetKeys());
        Assert.Equal(syncValue, asyncValue);
        Assert.Equal(syncCache.Hits, asyncCache.Hits);
        Assert.Equal(syncCache.Misses, asyncCache.Misses);
    }

    [SkippableFact]
    public void Should_PreserveStateAndOrdering_AfterRepeatedUpdates_WhenNotAtCapacity()
    {
        Skip.If(EvictionPolicy.Equals(EvictionPolicy.LRU), "LRU update moves the item to the front");

        // arrange

        var cache = CreateCache<int, string>(4);

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        var keysBeforeUpdates = cache.GetKeys();

        // act

        cache.AddOrUpdate(2, "updatedValue2");
        cache.AddOrUpdate(2, "updatedValue2_v2");

        // assert

        var keysAfterUpdates = cache.GetKeys();

        Assert.Equal("updatedValue2_v2", cache.Get(2));
        Assert.Contains(2, keysBeforeUpdates);
        Assert.Contains(2, keysAfterUpdates);
        Assert.Equal(keysBeforeUpdates, keysAfterUpdates);
    }

    [Fact]
    public void Should_StoreDistinctKeysCorrectly_WhenHashCodeCollisionsHappen()
    {
        // arrange

        var cache = CreateCache<CollisionKey, string>(3);

        var key1 = new CollisionKey(1);
        var key2 = new CollisionKey(2);
        var key3 = new CollisionKey(3);

        // act

        cache.Put(key1, "value1");
        cache.Put(key2, "value2");
        cache.Put(key3, "value3");

        // assert

        Assert.Equal(3, cache.Count);

        Assert.True(cache.TryGet(key1, out string key1Value));
        Assert.True(cache.TryGet(key2, out string key2Value));
        Assert.True(cache.TryGet(key3, out string key3Value));

        Assert.Equal("value1", key1Value);
        Assert.Equal("value2", key2Value);
        Assert.Equal("value3", key3Value);
    }

    [Fact]
    public void Should_IterateOverAllKeys_WhenHashCodeCollisionsHappen()
    {
        // arrange

        var cache = CreateCache<CollisionKey, string>(3);

        var key1 = new CollisionKey(1);
        var key2 = new CollisionKey(2);

        cache.Put(key1, "value1");
        cache.Put(key2, "value2");

        // act

        var cacheKeys = cache.GetKeys();
        var cacheValues = cache.GetSnapshot().Select(kvp => kvp.Value).ToImmutableList();

        // assert

        Assert.Equal(2, cacheKeys.Count);
        Assert.Contains(key1, cacheKeys);
        Assert.Contains(key2, cacheKeys);

        Assert.Equal(2, cacheValues.Count);
        Assert.Contains("value1", cacheValues);
        Assert.Contains("value2", cacheValues);
    }

    #region Metadata tests

    [Fact]
    public void Should_ContainCorrectMetadataValues_AfterPut()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        var beforePut = DateTimeOffset.UtcNow;
        cache.Put(1, "value1");
        var afterPut = DateTimeOffset.UtcNow;

        // act

        var meta = cache.GetMetadata(1);

        // assert

        Assert.InRange(meta.CreatedAt, beforePut, afterPut);
        Assert.InRange(meta.LastAccessedAt, beforePut, afterPut);
        Assert.InRange(meta.LastUpdatedAt, beforePut, afterPut);
        Assert.Equal(0, meta.AccessCount);
    }

    [Fact]
    public void Should_UpdateAccessFields_AndIncrementAccessCount_OnGet()
    {
        // arrange

        var cache = CreateCache<int, string>(2);
        cache.Put(1, "value1");

        long initialAccessCount = cache.GetMetadata(1).AccessCount;
        var initialAccess = cache.GetMetadata(1).LastAccessedAt;
        var initialUpdate = cache.GetMetadata(1).LastUpdatedAt;

        // act

        cache.Get(1);
        var metaAfter = cache.GetMetadata(1);

        // assert

        Assert.Equal(initialAccessCount + 1, metaAfter.AccessCount);
        Assert.True(metaAfter.LastAccessedAt >= initialAccess);
        Assert.Equal(initialUpdate, metaAfter.LastUpdatedAt);
    }

    [Fact]
    public void Should_UpdateLastUpdatedAt_WithoutChangingAccessCount_OnAddOrUpdate()
    {
        // arrange

        var cache = CreateCache<int, string>(2);
        cache.Put(1, "value1");

        long initialAccessCount = cache.GetMetadata(1).AccessCount;
        var initialAccess = cache.GetMetadata(1).LastAccessedAt;
        var initialUpdate = cache.GetMetadata(1).LastUpdatedAt;

        // act

        cache.AddOrUpdate(1, "value1Updated");
        var metaAfter = cache.GetMetadata(1);

        // assert

        Assert.True(metaAfter.LastUpdatedAt >= initialUpdate);
        Assert.Equal(initialAccess, metaAfter.LastAccessedAt);
        Assert.Equal(initialAccessCount, metaAfter.AccessCount);
    }

    [Fact]
    public void Should_TryGetMetadata_ReturnFalse_WhenKeyDoesNotExist()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        // act

        bool found = cache.TryGetMetadata(42, out var meta);

        // assert

        Assert.False(found);
        Assert.Null(meta);
    }

    #endregion
}
