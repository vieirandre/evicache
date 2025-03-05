using EviCache.Enums;
using EviCache.Options;
using EviCache.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace EviCache.Tests;

public abstract class CacheTestsBase
{
    protected abstract EvictionPolicy EvictionPolicy { get; }
    protected virtual bool SupportsEviction => true;
    protected readonly Mock<ILogger> _loggerMock;

    protected Cache<TKey, TValue> CreateCache<TKey, TValue>(int capacity, ILogger? logger = null) where TKey : notnull
    {
        var options = new CacheOptions(capacity, EvictionPolicy);
        return new Cache<TKey, TValue>(options, logger);
    }

    public CacheTestsBase()
    {
        _loggerMock = new Mock<ILogger>();
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
        Skip.If(EvictionPolicy.Equals(EvictionPolicy.FIFO), "FIFO doesn't reinsert on updates");

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

        var cache = CreateCache<int, string>(3);

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

        Assert.Equal("The key '3' wasn't found in the cache", exception.Message);
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

        var cache = CreateCache<int, string>(3);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        cache.Clear();
        bool containsKey1 = cache.ContainsKey(1);
        bool containsKey2 = cache.ContainsKey(2);

        // assert

        Assert.False(containsKey1);
        Assert.False(containsKey2);
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

        // act

        cache.Put(2, "value2");

        if (SupportsEviction)
        {
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
            // assert

            Assert.True(cache.TryGet(1, out _));
            Assert.False(cache.TryGet(2, out _));

            // act

            cache.Put(1, "newValue1");

            // assert

            Assert.True(cache.TryGet(1, out var updatedValue));
            Assert.Equal("newValue1", updatedValue);
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
                            cache.Put(key, value);
                            break;
                        case 1:
                            cache.TryGet(key, out _);
                            break;
                        case 2:
                            cache.Remove(key);
                            break;
                        case 3:
                            cache.AddOrUpdate(key, value);
                            break;
                        case 4:
                            cache.GetOrAdd(key, value);
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

        Assert.True(keys.SequenceEqual(snapshotKeys));
    }

    [Fact]
    public void Should_BeIdempotent_WhenClearingCacheMultipleTimes()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        cache.Clear();
        cache.Clear();

        // assert

        Assert.Equal(0, cache.Count);
        Assert.Empty(cache.GetKeys());
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
}
