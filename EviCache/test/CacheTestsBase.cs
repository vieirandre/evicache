using EviCache.Enums;
using EviCache.Options;
using EviCache.Tests.Utils;
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
        Skip.IfNot(SupportsEviction);

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
        Skip.IfNot(SupportsEviction);

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
        Skip.IfNot(SupportsEviction);

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
        Skip.IfNot(SupportsEviction);

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
        Skip.IfNot(SupportsEviction);

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
}
