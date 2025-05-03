using EviCache.Enums;
using EviCache.Exceptions;
using EviCache.Options;
using EviCache.Tests.Helpers;
using EviCache.Tests.Helpers.Disposal;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Immutable;

namespace EviCache.Tests;

public abstract class CacheAsyncTestsBase
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

    protected CacheAsyncTestsBase()
    {
        _loggerMock = new Mock<ILogger>();

        _loggerMock
            .Setup(logger => logger.IsEnabled(LogLevel.Debug))
            .Returns(true);
    }

    [Fact]
    public async Task Should_LogCacheClearInformation()
    {
        // arrange

        int capacity = 2;
        var cache = CreateCache<int, string>(capacity, _loggerMock.Object);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        // act

        await cache.ClearAsync();

        // assert

        _loggerMock.VerifyLog(LogLevel.Information, $"Cache cleared. Removed {capacity} items", Times.Once());
    }

    [Fact]
    public async Task Should_ReturnValue_WhenKeyIsInsertedAndRetrieved()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        // act

        await cache.PutAsync(1, "value");
        var (Found, Value) = await cache.TryGetAsync(1);

        // assert

        Assert.True(Found);
        Assert.Equal("value", Value);
    }

    [Fact]
    public async Task Should_StoreAndRetrieve_WhenKeyIsStringAndValueIsInt()
    {
        // arrange

        var cache = CreateCache<string, int>(2);

        // act

        await cache.PutAsync("this", 10);
        await cache.PutAsync("that", 20);
        var (Found, ThatValue) = await cache.TryGetAsync("that");

        // arrange

        Assert.True(Found);
        Assert.Equal(20, ThatValue);
    }

    [SkippableFact]
    public async Task Should_UpdateExistingKeyAndEvict_WhenReinsertingKeyOverCapacity()
    {
        Skip.IfNot(SupportsEviction, TestMessages.EvictionNotSupported);
        Skip.If(EvictionPolicy.Equals(EvictionPolicy.FIFO), "FIFO does not reorder on updates");

        // arrange

        var cache = CreateCache<int, string>(2);
        await cache.PutAsync(1, "oldValue");
        await cache.PutAsync(2, "value2");

        // act

        await cache.PutAsync(1, "newValue");
        await cache.PutAsync(3, "value3");

        // assert

        Assert.False((await cache.TryGetAsync(2)).Found);
        Assert.Equal("newValue", await cache.GetAsync(1));
        Assert.Equal("value3", await cache.GetAsync(3));
    }

    [Fact]
    public async Task Should_NotDisposeItem_WhenTryGetIsCalled()
    {
        // arrange

        var cache = CreateCache<int, DisposableDummy>(2);

        var disposableItem = new DisposableDummy();
        await cache.PutAsync(1, disposableItem);

        // act

        var (Found, Value) = await cache.TryGetAsync(1);

        // assert

        Assert.True(Found);
        Assert.False(disposableItem.IsDisposed);
        Assert.Equal(disposableItem, Value);
    }

    [SkippableFact]
    public async Task Should_NotExceedCapacity_WhenCacheIsFull()
    {
        Skip.IfNot(SupportsEviction, TestMessages.EvictionNotSupported);

        // arrange

        int capacity = 2;
        var cache = CreateCache<int, string>(capacity);

        // act

        for (int i = 0; i < capacity * 2; i++)
        {
            await cache.PutAsync(i, $"value{i}");
        }

        // assert

        Assert.True(cache.Count <= capacity);
    }

    [Fact]
    public async Task Should_RemoveKeyAndDisposeItem_WhenKeyExists()
    {
        // arrange

        var cache = CreateCache<int, DisposableDummy>(2);

        var disposableItem = new DisposableDummy();
        await cache.PutAsync(1, disposableItem);
        await cache.PutAsync(2, new DisposableDummy());

        // act

        bool removed = await cache.RemoveAsync(1);

        // assert

        Assert.True(removed);
        Assert.Equal(1, cache.Count);
        Assert.True(disposableItem.IsDisposed);
        Assert.False((await cache.TryGetAsync(1)).Found);
    }

    [Fact]
    public async Task Should_ReturnFalseAndNotDispose_WhenKeyDoesNotExist()
    {
        // arrange

        var cache = CreateCache<int, DisposableDummy>(2);

        var disposableItem = new DisposableDummy();
        await cache.PutAsync(1, disposableItem);

        // aact

        bool removed = await cache.RemoveAsync(9);

        // assert

        Assert.False(removed);
        Assert.False(disposableItem.IsDisposed);
    }

    [Fact]
    public async Task Should_ClearAllDisposableItems()
    {
        // arrange

        var cache = CreateCache<int, string>(3, _loggerMock.Object);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        // act

        await cache.ClearAsync();

        // assert

        Assert.Equal(0, cache.Count);
        Assert.False((await cache.TryGetAsync(1)).Found);
        Assert.False((await cache.TryGetAsync(2)).Found);
        Assert.False((await cache.TryGetAsync(3)).Found);

        _loggerMock.VerifyLog(LogLevel.Information, $"Cache cleared. Removed 3 items", Times.Once());
    }

    [SkippableFact]
    public async Task Should_TrackHitsMissesAndEvictions_WhenCacheCapacityIs15()
    {
        Skip.IfNot(SupportsEviction, TestMessages.EvictionNotSupported);

        // arrange

        int capacity = 15;
        var cache = CreateCache<int, string>(capacity);

        // act & assert

        for (int i = 1; i <= capacity; i++)
        {
            await cache.PutAsync(i, $"value{i}");
        }

        for (int i = 1; i <= 10; i++)
        {
            string value = await cache.GetAsync(i);
            Assert.Equal($"value{i}", value);
        }

        for (int i = 16; i <= 20; i++)
        {
            var (Found, _) = await cache.TryGetAsync(i);
            Assert.False(Found);
        }

        for (int i = 16; i <= 20; i++)
        {
            await cache.PutAsync(i, $"value{i}");
        }

        Assert.Equal(capacity, cache.Count);
        Assert.Equal(10, cache.Hits);
        Assert.Equal(5, cache.Misses);
        Assert.Equal(5, cache.Evictions);
    }

    [Fact]
    public async Task Should_AddNewValue_WhenKeyDoesNotExistInGetOrAdd()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        // act

        var result = await cache.GetOrAddAsync(1, "value1");

        // assert

        Assert.Equal("value1", result);

        var (Found, Value) = await cache.TryGetAsync(1);
        Assert.True(Found);
        Assert.Equal("value1", Value);
    }

    [Fact]
    public async Task Should_ReturnExistingValue_WhenKeyExistsInGetOrAdd()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        // act

        var result = await cache.GetOrAddAsync(1, "newValue1");

        // assert

        Assert.Equal("value1", result);
    }

    [SkippableFact]
    public async Task Should_Evict_WhenAddingNewItemExceedsCapacityInGetOrAdd()
    {
        Skip.IfNot(SupportsEviction, TestMessages.EvictionNotSupported);

        // arrange

        var cache = CreateCache<int, string>(2);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

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
    public async Task Should_AddNewValueAndNotEvict_WhenUnderCapacityInGetOrAdd()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        // act

        var result = await cache.GetOrAddAsync(3, "value3");

        // assert

        Assert.Equal("value3", result);
        Assert.Equal(3, cache.Count);

        var (Found1, Value1) = await cache.TryGetAsync(1);
        Assert.True(Found1);
        Assert.Equal("value1", Value1);

        var (Found2, Value2) = await cache.TryGetAsync(2);
        Assert.True(Found2);
        Assert.Equal("value2", Value2);

        var (Found3, Value3) = await cache.TryGetAsync(3);
        Assert.True(Found3);
        Assert.Equal("value3", Value3);
    }

    [Fact]
    public async Task Should_AddAndReturnNewValue_WhenUsingGetOrAddWithDisposableValue()
    {
        // arrange

        var cache = CreateCache<int, DisposableDummy>(2);

        // act

        var disposable1 = await cache.GetOrAddAsync(1, new DisposableDummy());

        // assert

        Assert.NotNull(disposable1);
        Assert.False(disposable1.IsDisposed);

        var (Found, Value) = await cache.TryGetAsync(1);
        Assert.True(Found);
        Assert.Equal(disposable1, Value);
    }

    [Fact]
    public async Task Should_ReturnValue_WhenKeyExists()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        // act

        var result = await cache.GetAsync(2);

        // assert

        Assert.Equal("value2", result);
    }

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_WhenKeyDoesNotExist()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        // act

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () => await cache.GetAsync(3));

        // assert

        Assert.Equal("The key '3' was not found in the cache", exception.Message);
    }

    [Fact]
    public async Task Should_NotDisposeItem_WhenGetIsCalled()
    {
        // arrange

        var cache = CreateCache<int, DisposableDummy>(2);

        var disposableItem = new DisposableDummy();
        await cache.PutAsync(1, disposableItem);

        // act

        var result = await cache.GetAsync(1);

        // assert

        Assert.False(disposableItem.IsDisposed);
        Assert.Equal(disposableItem, result);
    }

    [Fact]
    public async Task Should_UpdateExistingKey_AndReturnNewValue()
    {
        // arrange

        var cache = CreateCache<int, string>(5);

        await cache.PutAsync(1, "oldValue");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        // act

        var result = await cache.AddOrUpdateAsync(1, "newValue");
        await cache.GetAsync(3);

        // assert

        Assert.Equal("newValue", result);
        Assert.Equal("newValue", await cache.GetAsync(1));
    }

    [Fact]
    public async Task Should_AddNewKey_AndReturnValue()
    {
        // arrange

        var cache = CreateCache<int, string>(5);

        // act

        var result = await cache.AddOrUpdateAsync(2, "value2");

        // assert

        Assert.Equal("value2", result);
        Assert.Equal("value2", await cache.GetAsync(2));
        Assert.Equal(1, cache.Count);
    }

    [SkippableFact]
    public async Task Should_Evict_WhenAddingNewKeyExceedingCapacity()
    {
        Skip.IfNot(SupportsEviction, TestMessages.EvictionNotSupported);

        // arrange

        var cache = CreateCache<int, string>(3);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");
        await cache.GetAsync(2);

        // act

        var result = await cache.AddOrUpdateAsync(4, "value4");

        // assert

        Assert.Equal("value4", result);
        Assert.Equal(3, cache.Count);
        Assert.False((await cache.TryGetAsync(1)).Found);
        Assert.Equal(1, cache.Evictions);
    }

    [Fact]
    public async Task Should_UpdateMetrics_OnAddOrUpdate_ForExistingAndNewKeys()
    {
        // arrange

        var cache = CreateCache<int, string>(5);

        // act & assert

        await cache.AddOrUpdateAsync(1, "value1");
        Assert.Equal(1, cache.Misses);
        Assert.Equal(0, cache.Hits);

        await cache.AddOrUpdateAsync(1, "value1Updated");
        Assert.Equal(1, cache.Hits);

        await cache.AddOrUpdateAsync(2, "value2");
        Assert.Equal(2, cache.Misses);
    }

    [Fact]
    public async Task ContainsKey_ReturnsTrue_WhenKeyExists()
    {
        // arrange

        var cache = CreateCache<int, string>(3);
        await cache.PutAsync(1, "value1");

        // act

        bool contains = await cache.ContainsKeyAsync(1);

        // assert

        Assert.True(contains);
    }

    [Fact]
    public async Task ContainsKey_ReturnsFalse_WhenKeyDoesNotExist()
    {
        // arrange

        var cache = CreateCache<int, string>(3);
        await cache.PutAsync(1, "value1");

        // act

        bool contains = await cache.ContainsKeyAsync(2);

        // assert

        Assert.False(contains);
    }

    [Fact]
    public async Task ContainsKey_ReturnsFalse_AfterKeyIsRemoved()
    {
        // arrange

        var cache = CreateCache<int, string>(3);
        await cache.PutAsync(1, "value1");

        // act

        bool removed = await cache.RemoveAsync(1);
        bool contains = await cache.ContainsKeyAsync(1);

        // assert

        Assert.True(removed);
        Assert.False(contains);
    }

    [Fact]
    public async Task ContainsKey_ReturnsFalse_WhenCacheIsCleared()
    {
        // arrange

        var cache = CreateCache<int, string>(3, _loggerMock.Object);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        // act

        await cache.ClearAsync();
        bool containsKey1 = await cache.ContainsKeyAsync(1);
        bool containsKey2 = await cache.ContainsKeyAsync(2);

        // assert

        Assert.False(containsKey1);
        Assert.False(containsKey2);

        _loggerMock.VerifyLog(LogLevel.Information, $"Cache cleared. Removed 2 items", Times.Once());
    }

    [Fact]
    public async Task Should_HandleCapacityOfOne()
    {
        // arrange

        var cache = CreateCache<int, string>(1, _loggerMock.Object);

        // act

        await cache.PutAsync(1, "value1");

        // assert

        var (Found1, Value1) = await cache.TryGetAsync(1);
        Assert.True(Found1);
        Assert.Equal("value1", Value1);

        if (SupportsEviction)
        {
            // act

            await cache.PutAsync(2, "value2");

            // assert

            Assert.False((await cache.TryGetAsync(1)).Found);

            var (Found2, Value2) = await cache.TryGetAsync(2);
            Assert.True(Found2);
            Assert.Equal("value2", Value2);

            // act

            await cache.PutAsync(2, "newValue2");

            // assert

            var (FoundUpdated2, ValueUpdated2) = await cache.TryGetAsync(2);
            Assert.True(FoundUpdated2);
            Assert.Equal("newValue2", ValueUpdated2);
        }
        else
        {
            // act

            var exception = await Assert.ThrowsAsync<CacheFullException>(async () => await cache.PutAsync(2, "value2"));

            // assert

            Assert.True((await cache.TryGetAsync(1)).Found);
            Assert.False((await cache.TryGetAsync(2)).Found);

            // act

            await cache.PutAsync(1, "newValue1");

            // assert

            var (FoundUpdated1, ValueUpdated1) = await cache.TryGetAsync(1);
            Assert.True(FoundUpdated1);
            Assert.Equal("newValue1", ValueUpdated1);
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
            tasks.Add(Task.Run(async () =>
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
                            try { await cache.PutAsync(key, value); }
                            catch (CacheFullException ex) { putException ??= ex; }
                            break;
                        case 1:
                            await cache.TryGetAsync(key);
                            break;
                        case 2:
                            await cache.RemoveAsync(key);
                            break;
                        case 3:
                            try { await cache.AddOrUpdateAsync(key, value); }
                            catch (CacheFullException ex) { addOrUpdateException ??= ex; }
                            break;
                        case 4:
                            try { await cache.GetOrAddAsync(key, value); }
                            catch (CacheFullException ex) { getOrAddException ??= ex; }
                            break;
                        default:
                            // nothing
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
            Assert.True((await cache.TryGetAsync(key)).Found);
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
        await cache.PutAsync(key, 0);

        int taskCount = 50;
        int iterations = 1000;

        var tasks = new List<Task>();

        // act

        for (int t = 0; t < taskCount; t++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    await cache.AddOrUpdateAsync(key, i);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // assert

        Assert.Equal(1, cache.Count);
        Assert.Equal(0, cache.Evictions);

        var (Found, FinalValue) = await cache.TryGetAsync(key);
        Assert.True(Found);
        Assert.InRange(FinalValue, 0, iterations - 1);

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
            tasks.Add(Task.Run(async () =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    await cache.PutAsync(key, i);
                    await cache.RemoveAsync(key);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // assert

        Assert.True(cache.Count <= 1);
        Assert.True(cache.GetKeys().Count <= 1);
    }

    [Fact]
    public async Task Should_ReturnConsistentKeysAndSnapshot()
    {
        // arrange

        int capacity = 15;
        var cache = CreateCache<int, string>(capacity);

        var numbers = Enumerable.Range(1, capacity);
        numbers.Shuffle();

        foreach (int n in numbers)
        {
            await cache.PutAsync(n, $"value{n}");
        }

        // act

        var keys = cache.GetKeys();
        var snapshotKeys = cache.GetSnapshot().Select(kvp => kvp.Key);

        // assert

        Assert.Equivalent(keys, snapshotKeys);
    }

    [Fact]
    public async Task Should_BeIdempotent_WhenClearingCacheMultipleTimes()
    {
        // arrange

        var cache = CreateCache<int, string>(3, _loggerMock.Object);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

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
    public async Task Should_UpdateMetricsAccurately_AfterMixedOperations()
    {
        // arrange

        var cache = CreateCache<int, string>(5);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        // act

        await cache.GetAsync(1);
        await cache.GetAsync(2);

        // act & assert

        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await cache.GetAsync(4));

        // act

        await cache.TryGetAsync(4);
        await cache.AddOrUpdateAsync(1, "newValue1");
        await cache.RemoveAsync(2);
        await cache.GetOrAddAsync(5, "value5");

        // assert

        Assert.Equal(3, cache.Count);
        Assert.Equal(3, cache.Hits);
        Assert.Equal(3, cache.Misses);
    }

    [Fact]
    public async Task Should_NotImpactMetrics_WhenContainsKeyIsCalled()
    {
        // arrange

        var cache = CreateCache<int, string>(3);
        await cache.PutAsync(1, "value1");

        // act

        bool containsExisting = await cache.ContainsKeyAsync(1);
        bool containsMissing = await cache.ContainsKeyAsync(99);

        // assert

        Assert.True(containsExisting);
        Assert.False(containsMissing);
        Assert.Equal(0, cache.Hits);
        Assert.Equal(0, cache.Misses);
    }

    [SkippableFact]
    public async Task Should_PreserveStateAndOrdering_AfterRepeatedUpdates_WhenNotAtCapacity()
    {
        Skip.If(EvictionPolicy.Equals(EvictionPolicy.LRU), "LRU update moves the item to the front");

        // arrange

        var cache = CreateCache<int, string>(4);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        var keysBeforeUpdates = cache.GetKeys();

        // act

        await cache.AddOrUpdateAsync(2, "updatedValue2");
        await cache.AddOrUpdateAsync(2, "updatedValue2_v2");

        // assert

        var keysAfterUpdates = cache.GetKeys();

        Assert.Equal("updatedValue2_v2", await cache.GetAsync(2));
        Assert.Contains(2, keysBeforeUpdates);
        Assert.Contains(2, keysAfterUpdates);
        Assert.Equal(keysBeforeUpdates, keysAfterUpdates);
    }

    [Fact]
    public async Task Should_StoreDistinctKeysCorrectly_WhenHashCodeCollisionsHappen()
    {
        // arrange

        var cache = CreateCache<CollisionKey, string>(3);

        var key1 = new CollisionKey(1);
        var key2 = new CollisionKey(2);
        var key3 = new CollisionKey(3);

        // act

        await cache.PutAsync(key1, "value1");
        await cache.PutAsync(key2, "value2");
        await cache.PutAsync(key3, "value3");

        // assert

        Assert.Equal(3, cache.Count);

        var (FoundKey1, ValueKey1) = await cache.TryGetAsync(key1);
        Assert.True(FoundKey1);

        var (FoundKey2, ValueKey2) = await cache.TryGetAsync(key2);
        Assert.True(FoundKey2);

        var (FoundKey3, ValueKey3) = await cache.TryGetAsync(key3);
        Assert.True(FoundKey3);

        Assert.Equal("value1", ValueKey1);
        Assert.Equal("value2", ValueKey2);
        Assert.Equal("value3", ValueKey3);
    }

    [Fact]
    public async Task Should_IterateOverAllKeys_WhenHashCodeCollisionsHappen()
    {
        // arrange

        var cache = CreateCache<CollisionKey, string>(3);

        var key1 = new CollisionKey(1);
        var key2 = new CollisionKey(2);

        await cache.PutAsync(key1, "value1");
        await cache.PutAsync(key2, "value2");

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
}
