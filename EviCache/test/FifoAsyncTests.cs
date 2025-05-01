using EviCache.Enums;
using EviCache.Exceptions;
using EviCache.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace EviCache.Tests;

public class FifoAsyncTests : CacheAsyncTestsBase
{
    protected override EvictionPolicy EvictionPolicy => EvictionPolicy.FIFO;

    [Fact]
    public async Task Should_EvictFirstInsertedItem_WhenCapacityExceeded()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        // act

        await cache.PutAsync(3, "value3");

        // assert

        Assert.False((await cache.TryGetAsync(1)).Found);

        var (Found2, Value2) = await cache.TryGetAsync(2);
        Assert.True(Found2);
        Assert.Equal("value2", Value2);

        var (Found3, Value3) = await cache.TryGetAsync(3);
        Assert.True(Found3);
        Assert.Equal("value3", Value3);

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 1 | Total evictions: 1", Times.Once());
    }

    [Fact]
    public async Task Should_MaintainFifoOrder_AfterInsertionsAndRemovals()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        // act

        await cache.RemoveAsync(1);
        await cache.PutAsync(4, "value4");

        // assert

        Assert.Equal(new[] { 2, 3, 4 }, cache.GetKeys());
    }

    [Fact]
    public async Task Should_NotChangeOrder_OnAccessOrUpdate()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        // act

        await cache.GetAsync(1);
        await cache.PutAsync(2, "newValue2");

        // assert

        Assert.Equal(new[] { 1, 2, 3 }, cache.GetKeys());
        Assert.Equal("value1", await cache.GetAsync(1));
        Assert.Equal("newValue2", await cache.GetAsync(2));
        Assert.Equal("value3", await cache.GetAsync(3));
    }

    [Fact]
    public async Task Should_ThrowException_WhenEvictionFails_DueToNoCandidate()
    {
        // arrange

        var cache = CreateCache<int, string>(1, _loggerMock.Object);
        await cache.PutAsync(1, "value1");

        await cache.OverrideEvictionCandidateCollection(EvictionPolicy.GetEvictionCandidateCollectionFieldName(), new LinkedList<int>());

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

        await cache.OverrideEvictionCandidateCollection(EvictionPolicy.GetEvictionCandidateCollectionFieldName(), fakeCandidateList);

        // act & assert

        var ex = await Assert.ThrowsAsync<CacheFullException>(async () => await cache.PutAsync(2, "value2"));

        Assert.Equal("Cache is full (capacity: 1). Failed to evict any item while adding key: 2", ex.Message);
        _loggerMock.VerifyLog(LogLevel.Error, $"Eviction candidate ({fakeCandidate}) was not found in the cache", Times.Once());
    }
}
