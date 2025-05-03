using EviCache.Enums;
using EviCache.Exceptions;
using EviCache.Tests.Helpers;
using EviCache.Tests.Helpers.Disposal;
using Microsoft.Extensions.Logging;
using Moq;

namespace EviCache.Tests;

public class LruAsyncTests : CacheAsyncTestsBase
{
    protected override EvictionPolicy EvictionPolicy => EvictionPolicy.LRU;

    [Fact]
    public async Task Should_EvictLeastRecentlyUsed_WhenCapacityIsExceeded()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        // act

        var (_, Value1) = await cache.TryGetAsync(1);
        await cache.PutAsync(3, "value3");
        var (_, Value2) = await cache.TryGetAsync(2);
        var (_, Value3) = await cache.TryGetAsync(3);

        // assert

        Assert.Null(Value2);
        Assert.Equal("value1", Value1);
        Assert.Equal("value3", Value3);

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 2 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public async Task Should_UpdateExistingKeyAndEvictLeastRecentlyUsed_WhenReinsertingKeyOverCapacity()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        await cache.PutAsync(1, "oldValue");
        await cache.PutAsync(2, "value2");

        // act

        await cache.PutAsync(1, "newValue");
        await cache.PutAsync(3, "value3");

        // assert

        Assert.Equal("newValue", (await cache.TryGetAsync(1)).Value);
        Assert.Null((await cache.TryGetAsync(2)).Value);
        Assert.Equal("value3", (await cache.TryGetAsync(3)).Value);

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 2 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public async Task Should_EvictLeastRecentlyUsed_MultipleTimes()
    {
        // arrange

        var cache = CreateCache<int, string>(3, _loggerMock.Object);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        await cache.GetAsync(2);

        // act

        await cache.PutAsync(4, "value4");

        // assert

        Assert.False((await cache.TryGetAsync(1)).Found);
        Assert.True((await cache.TryGetAsync(2)).Found);
        Assert.True((await cache.TryGetAsync(3)).Found);
        Assert.True((await cache.TryGetAsync(4)).Found);
        Assert.Equal(1, cache.Evictions);

        // act

        for (int i = 5; i <= 10; i++)
        {
            await cache.PutAsync(i, $"value{i}");
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
    public async Task Should_ClearAllItems_WhenClearIsCalled()
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

    [Fact]
    public async Task Should_ReturnExistingValueAndMoveToFront_WhenKeyExistsInGetOrAdd()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        // act

        var result = await cache.GetOrAddAsync(1, "newValue1");

        // assert

        Assert.Equal("value1", result);
        var keys = cache.GetKeys();
        Assert.Equal([1, 2], keys);
    }

    [Fact]
    public async Task Should_EvictLeastRecentlyUsed_WhenAddingNewItemExceedsCapacityInGetOrAdd()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        // act

        var result = cache.GetOrAdd(3, "value3");

        // assert

        Assert.Equal("value3", result);
        Assert.False((await cache.TryGetAsync(1)).Found); // evicted

        var (Found2, Value2) = await cache.TryGetAsync(2);
        Assert.True(Found2);
        Assert.Equal("value2", Value2);

        var (Found3, Value3) = await cache.TryGetAsync(3);
        Assert.True(Found3);
        Assert.Equal("value3", Value3);

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 1 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public async Task Should_DisposeEvictedItem_WhenAddingNewItemExceedsCapacityWithDisposableValue()
    {
        // arrange

        var cache = CreateCache<int, DisposableDummy>(2, _loggerMock.Object);

        var disposable1 = new DisposableDummy();
        var disposable2 = new DisposableDummy();

        await cache.PutAsync(1, disposable1);
        await cache.PutAsync(2, disposable2);

        // act

        var disposable3 = await cache.GetOrAddAsync(3, new DisposableDummy());

        // assert

        Assert.True(disposable1.IsDisposed);
        Assert.False(disposable2.IsDisposed);

        var (Found2, Value2) = await cache.TryGetAsync(2);
        Assert.True(Found2);
        Assert.Equal(disposable2, Value2);

        var (Found3, Value3) = await cache.TryGetAsync(3);
        Assert.True(Found3);
        Assert.Equal(disposable3, Value3);

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 1 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public async Task Should_ReturnEmptyList_WhenCacheIsCleared()
    {
        // arrange

        var cache = CreateCache<int, string>(3, _loggerMock.Object);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        // act

        await cache.ClearAsync();
        var keys = cache.GetKeys();

        // assert

        Assert.Empty(keys);
        Assert.Equal(0, cache.Count);

        _loggerMock.VerifyLog(LogLevel.Information, $"Cache cleared. Removed 3 items", Times.Once());
    }

    [Fact]
    public async Task Should_EvictLeastRecentlyUsed_WhenAddingNewKeyExceedingCapacity()
    {
        // arrange

        var cache = CreateCache<int, string>(3, _loggerMock.Object);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        // act

        await cache.GetAsync(2);
        var result = await cache.AddOrUpdateAsync(4, "value4");

        // assert

        Assert.Equal("value4", result);
        Assert.Equal(3, cache.Count);
        Assert.False((await cache.TryGetAsync(1)).Found);
        Assert.Equal(1, cache.Evictions);

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 1 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public async Task Should_ReturnKeysInMruToLruOrder_WhenItemsAreAdded()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        // act

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        // assert

        Assert.Collection(cache.GetKeys(),
            key => Assert.Equal(3, key),
            key => Assert.Equal(2, key),
            key => Assert.Equal(1, key));
    }

    [Fact]
    public async Task Should_UpdateKeyOrder_WhenItemsAreAccessed()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        // act

        cache.TryGet(1, out _);

        // assert

        Assert.Collection(cache.GetKeys(),
            key => Assert.Equal(1, key),
            key => Assert.Equal(3, key),
            key => Assert.Equal(2, key));
    }

    [Fact]
    public async Task Should_RemoveEvictedKeyFromOrder_WhenCapacityIsExceeded()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        // act

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        // assert

        Assert.Collection(cache.GetKeys(),
            key => Assert.Equal(3, key),
            key => Assert.Equal(2, key));

        Assert.False((await cache.TryGetAsync(1)).Found);

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 1 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public async Task Should_RemoveKeyFromOrder_WhenItemIsRemoved()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        // act

        await cache.RemoveAsync(2);

        // assert

        Assert.Collection(cache.GetKeys(),
            key => Assert.Equal(3, key),
            key => Assert.Equal(1, key));

        Assert.False((await cache.TryGetAsync(2)).Found);
    }

    [Fact]
    public async Task Should_NotDuplicateKeysInOrder_WhenAddingExistingKey()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        // act

        await cache.PutAsync(2, "newValue2");

        // assert

        Assert.Collection(cache.GetKeys(),
            key => Assert.Equal(2, key),
            key => Assert.Equal(3, key),
            key => Assert.Equal(1, key));

        var (Found2, Value2) = await cache.TryGetAsync(2);
        Assert.Equal("newValue2", Found2 ? Value2 : null);
    }

    [Fact]
    public async Task Should_HandleGetKeys_WithDisposableItems()
    {
        // arrange

        var cache = CreateCache<int, DisposableDummy>(3);

        var disposable1 = new DisposableDummy();
        var disposable2 = new DisposableDummy();
        var disposable3 = new DisposableDummy();

        // act

        await cache.PutAsync(1, disposable1);
        await cache.PutAsync(2, disposable2);
        await cache.PutAsync(3, disposable3);
        _ = await cache.TryGetAsync(1);

        // assert

        Assert.Collection(cache.GetKeys(),
            key => Assert.Equal(1, key),
            key => Assert.Equal(3, key),
            key => Assert.Equal(2, key));

        Assert.False(disposable1.IsDisposed);
    }

    [Fact]
    public async Task Should_MoveKeyToFront_WhenGetIsCalled()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        // act

        var result = await cache.GetAsync(1);

        // assert

        Assert.Equal("value1", result);

        Assert.Collection(cache.GetKeys(),
            key => Assert.Equal(1, key),
            key => Assert.Equal(3, key),
            key => Assert.Equal(2, key));
    }

    [Fact]
    public async Task Should_ContainAllItemsRegardlessOfOrder()
    {
        // arrange

        const int capacity = 15;
        var cache = CreateCache<int, string>(capacity);

        for (int i = 1; i <= capacity; i++)
        {
            await cache.PutAsync(i, $"value{i}");
        }

        var expected = Enumerable.Range(1, capacity)
            .Select(i => KeyValuePair.Create(i, $"value{i}"))
            .ToList();

        // act

        var snapshot = cache.GetSnapshot();

        // assert

        Assert.Equal(expected.Count, snapshot.Count);
        Assert.True(expected.All(snapshot.Contains));
    }

    [Fact]
    public async Task Should_MaintainCorrectOrder_WithRepeatedAccesses()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        // assert

        Assert.Equal(new[] { 3, 2, 1 }, cache.GetKeys());

        // act & assert

        cache.Get(2);
        Assert.Equal(new[] { 2, 3, 1 }, cache.GetKeys());

        cache.Get(2);
        Assert.Equal(new[] { 2, 3, 1 }, cache.GetKeys());

        cache.Get(1);
        Assert.Equal(new[] { 1, 2, 3 }, cache.GetKeys());

        cache.Get(3);
        Assert.Equal(new[] { 3, 1, 2 }, cache.GetKeys());

        cache.Get(3);
        Assert.Equal(new[] { 3, 1, 2 }, cache.GetKeys());

        cache.Get(2);
        Assert.Equal(new[] { 2, 3, 1 }, cache.GetKeys());
    }

    [Fact]
    public async Task Should_ThrowException_WhenEvictionFails_DueToNoCandidate()
    {
        // arrange

        var cache = CreateCache<int, string>(1, _loggerMock.Object);
        await cache.PutAsync(1, "value1");

        cache.OverrideEvictionCandidateCollection(EvictionPolicy.GetEvictionCandidateCollectionFieldName(), new LinkedList<int>());

        // act & assert

        var ex = await Assert.ThrowsAsync<CacheFullException>(async () => await cache.PutAsync(2, "value2"));

        Assert.Equal("Cache is full (capacity: 1). Failed to evict any item while adding key: 2", ex.Message);
        _loggerMock.VerifyLog(LogLevel.Error, "Eviction selector did not return a candidate", Times.Once());
    }

    [Fact]
    public async Task Should_ThrowException_WhenEvictionFails_DueToCandidateNotInCache()
    {
        // arrange

        var cache = CreateCache<int, string>(1, _loggerMock.Object);
        await cache.PutAsync(1, "value1");

        int fakeCandidate = 999;
        var fakeCandidateList = new LinkedList<int>();
        fakeCandidateList.AddLast(fakeCandidate);

        cache.OverrideEvictionCandidateCollection(EvictionPolicy.GetEvictionCandidateCollectionFieldName(), fakeCandidateList);

        // act & assert

        var ex = Assert.Throws<CacheFullException>(() => cache.Put(2, "value2"));

        Assert.Equal("Cache is full (capacity: 1). Failed to evict any item while adding key: 2", ex.Message);
        _loggerMock.VerifyLog(LogLevel.Error, $"Eviction candidate ({fakeCandidate}) was not found in the cache", Times.Once());
    }
}