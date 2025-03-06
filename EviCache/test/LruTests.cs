using EviCache.Enums;
using EviCache.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace EviCache.Tests;

public class LruTests : CacheTestsBase
{
    protected override EvictionPolicy EvictionPolicy => EvictionPolicy.LRU;

    [Fact]
    public void Should_EvictLeastRecentlyUsed_WhenCapacityIsExceeded()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        cache.TryGet(1, out var value1);
        cache.Put(3, "value3");
        cache.TryGet(2, out var value2);
        cache.TryGet(3, out var value3);

        // assert

        Assert.Null(value2);
        Assert.Equal("value1", value1);
        Assert.Equal("value3", value3);

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 2 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public void Should_UpdateExistingKeyAndEvictLeastRecentlyUsed_WhenReinsertingKeyOverCapacity()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        cache.Put(1, "oldValue");
        cache.Put(2, "value2");

        // act

        cache.Put(1, "newValue");
        cache.Put(3, "value3");

        // assert

        cache.TryGet(1, out var value1);
        Assert.Equal("newValue", value1);

        cache.TryGet(2, out var value2);
        Assert.Null(value2);

        cache.TryGet(3, out var value3);
        Assert.Equal("value3", value3);

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 2 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public void Should_EvictLeastRecentlyUsed_MultipleTimes()
    {
        // arrange

        var cache = CreateCache<int, string>(3, _loggerMock.Object);

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        cache.Get(2);

        // act

        cache.Put(4, "value4");

        // assert

        Assert.False(cache.TryGet(1, out _));
        Assert.True(cache.TryGet(2, out _));
        Assert.True(cache.TryGet(3, out _));
        Assert.True(cache.TryGet(4, out _));
        Assert.Equal(1, cache.Evictions);

        // act

        for (int i = 5; i <= 10; i++)
        {
            cache.Put(i, $"value{i}");
        }

        // assert

        Assert.Equal(3, cache.Count);
        Assert.Equal(7, cache.Evictions);

        var keys = cache.GetKeys();
        Assert.Equal(10, keys[0]);

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 1 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 7 | Total evictions: 7", Times.Once());
        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: .* | Total evictions: .*", Times.Exactly(7));
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public void Should_ClearAllItems_WhenClearIsCalled()
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

    [Fact]
    public void Should_ReturnExistingValueAndMoveToFront_WhenKeyExistsInGetOrAdd()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        var result = cache.GetOrAdd(1, "newValue1");

        // assert

        Assert.Equal("value1", result);
        var keys = cache.GetKeys();
        Assert.Equal([1, 2], keys);
    }

    [Fact]
    public void Should_EvictLeastRecentlyUsed_WhenAddingNewItemExceedsCapacityInGetOrAdd()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        var result = cache.GetOrAdd(3, "value3");

        // assert

        Assert.Equal("value3", result);
        Assert.False(cache.TryGet(1, out _)); // evicted
        Assert.True(cache.TryGet(2, out var value2));
        Assert.Equal("value2", value2);
        Assert.True(cache.TryGet(3, out var value3));
        Assert.Equal("value3", value3);

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 1 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public void Should_DisposeEvictedItem_WhenAddingNewItemExceedsCapacityWithDisposableValue()
    {
        // arrange

        var cache = CreateCache<int, DisposableDummy>(2, _loggerMock.Object);

        var disposable1 = new DisposableDummy();
        var disposable2 = new DisposableDummy();

        cache.Put(1, disposable1);
        cache.Put(2, disposable2);

        // act

        var disposable3 = cache.GetOrAdd(3, new DisposableDummy());

        // assert

        Assert.True(disposable1.IsDisposed);
        Assert.False(disposable2.IsDisposed);
        Assert.True(cache.TryGet(2, out var retrieved2));
        Assert.Equal(disposable2, retrieved2);
        Assert.True(cache.TryGet(3, out var retrieved3));
        Assert.Equal(disposable3, retrieved3);

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 1 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public void Should_ReturnEmptyList_WhenCacheIsCleared()
    {
        // arrange

        var cache = CreateCache<int, string>(3, _loggerMock.Object);

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        cache.Clear();
        var keys = cache.GetKeys();

        // assert

        Assert.Empty(keys);
        Assert.Equal(0, cache.Count);

        _loggerMock.VerifyLog(LogLevel.Information, $"Cache cleared. Removed 3 items", Times.Once());
    }

    [Fact]
    public void Should_EvictLeastRecentlyUsed_WhenAddingNewKeyExceedingCapacity()
    {
        // arrange

        var cache = CreateCache<int, string>(3, _loggerMock.Object);

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        cache.Get(2);
        var result = cache.AddOrUpdate(4, "value4");

        // assert

        Assert.Equal("value4", result);
        Assert.Equal(3, cache.Count);
        Assert.False(cache.TryGet(1, out _));
        Assert.Equal(1, cache.Evictions);

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 1 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public void Should_ReturnKeysInMruToLruOrder_WhenItemsAreAdded()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        // act

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // assert

        Assert.Collection(cache.GetKeys(),
            key => Assert.Equal(3, key),
            key => Assert.Equal(2, key),
            key => Assert.Equal(1, key));
    }

    [Fact]
    public void Should_UpdateKeyOrder_WhenItemsAreAccessed()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        cache.TryGet(1, out _);

        // assert

        Assert.Collection(cache.GetKeys(),
            key => Assert.Equal(1, key),
            key => Assert.Equal(3, key),
            key => Assert.Equal(2, key));
    }

    [Fact]
    public void Should_RemoveEvictedKeyFromOrder_WhenCapacityIsExceeded()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        // act

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // assert

        Assert.Collection(cache.GetKeys(),
            key => Assert.Equal(3, key),
            key => Assert.Equal(2, key));

        Assert.False(cache.TryGet(1, out _));

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 1 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public void Should_RemoveKeyFromOrder_WhenItemIsRemoved()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        cache.Remove(2);

        // assert

        Assert.Collection(cache.GetKeys(),
            key => Assert.Equal(3, key),
            key => Assert.Equal(1, key));

        Assert.False(cache.TryGet(2, out _));
    }

    [Fact]
    public void Should_NotDuplicateKeysInOrder_WhenAddingExistingKey()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        cache.Put(2, "newValue2");

        // assert

        Assert.Collection(cache.GetKeys(),
            key => Assert.Equal(2, key),
            key => Assert.Equal(3, key),
            key => Assert.Equal(1, key));

        Assert.Equal("newValue2", cache.TryGet(2, out var value2) ? value2 : null);
    }

    [Fact]
    public void Should_HandleGetKeys_WithDisposableItems()
    {
        // arrange

        var cache = CreateCache<int, DisposableDummy>(3);

        var disposable1 = new DisposableDummy();
        var disposable2 = new DisposableDummy();
        var disposable3 = new DisposableDummy();

        // act

        cache.Put(1, disposable1);
        cache.Put(2, disposable2);
        cache.Put(3, disposable3);
        cache.TryGet(1, out _);

        // assert

        Assert.Collection(cache.GetKeys(),
            key => Assert.Equal(1, key),
            key => Assert.Equal(3, key),
            key => Assert.Equal(2, key));

        Assert.False(disposable1.IsDisposed);
    }

    [Fact]
    public void Should_MoveKeyToFront_WhenGetIsCalled()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        var result = cache.Get(1);

        // assert

        Assert.Equal("value1", result);

        Assert.Collection(cache.GetKeys(),
            key => Assert.Equal(1, key),
            key => Assert.Equal(3, key),
            key => Assert.Equal(2, key));
    }

    [Fact]
    public void Should_ReturnOrderedSnapshot_WhenCacheContains15Items()
    {
        // arrange

        int capacity = 15;
        var expectedOrder = Enumerable.Range(1, capacity).Reverse().ToArray();

        var cache = CreateCache<int, string>(capacity);

        for (int i = 1; i <= capacity; i++)
        {
            cache.Put(i, $"value{i}");
        }

        // act

        var snapshot = cache.GetSnapshot();

        // assert

        Assert.Equal(capacity, snapshot.Count);

        var assertions = expectedOrder.Select(expected => (Action<KeyValuePair<int, string>>)(item =>
        {
            Assert.Equal(expected, item.Key);
            Assert.Equal($"value{expected}", item.Value);
        })).ToArray();

        Assert.Collection(snapshot, assertions);
    }
}