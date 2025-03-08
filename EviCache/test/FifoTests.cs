using EviCache.Enums;
using EviCache.Exceptions;
using EviCache.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace EviCache.Tests;

public class FifoTests : CacheTestsBase
{
    protected override EvictionPolicy EvictionPolicy => EvictionPolicy.FIFO;

    [Fact]
    public void Should_EvictFirstInsertedItem_WhenCapacityExceeded()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        cache.Put(3, "value3");

        // assert

        Assert.False(cache.TryGet(1, out _));
        Assert.True(cache.TryGet(2, out var value2));
        Assert.Equal("value2", value2);
        Assert.True(cache.TryGet(3, out var value3));
        Assert.Equal("value3", value3);

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 1 | Total evictions: 1", Times.Once());
    }

    [Fact]
    public void Should_MaintainFifoOrder_AfterInsertionsAndRemovals()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        cache.Remove(1);
        cache.Put(4, "value4");

        // assert

        Assert.Equal(new[] { 2, 3, 4 }, cache.GetKeys());
    }

    [Fact]
    public void Should_NotChangeOrder_OnAccessOrUpdate()
    {
        // arrange

        var cache = CreateCache<int, string>(3);

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        cache.Get(1);
        cache.Put(2, "newValue2");

        // assert

        Assert.Equal(new[] { 1, 2, 3 }, cache.GetKeys());
        Assert.Equal("value1", cache.Get(1));
        Assert.Equal("newValue2", cache.Get(2));
        Assert.Equal("value3", cache.Get(3));
    }

    [Fact]
    public void Should_ThrowException_WhenEvictionFails_DueToNoCandidate()
    {
        // arrange

        var cache = CreateCache<int, string>(1, _loggerMock.Object);
        cache.Put(1, "value1");

        cache.OverrideEvictionCandidateCollection(EvictionPolicy.GetEvictionCandidateCollectionFieldName(), new LinkedList<int>());

        // act & assert

        var ex = Assert.Throws<CacheFullException>(() => cache.Put(2, "value2"));

        Assert.Equal("Cache is full (capacity: 1) and eviction failed for key: 2", ex.Message);
        _loggerMock.VerifyLog(LogLevel.Error, "Eviction selector did not return a candidate", Times.Once());
    }

    [Fact]
    public void Should_ThrowException_WhenEvictionFails_DueToCandidateNotInCache()
    {
        // arrange

        var cache = CreateCache<int, string>(1, _loggerMock.Object);
        cache.Put(1, "value1");

        int fakeCandidate = 999;
        var fakeCandidateList = new LinkedList<int>();
        fakeCandidateList.AddLast(fakeCandidate);

        cache.OverrideEvictionCandidateCollection(EvictionPolicy.GetEvictionCandidateCollectionFieldName(), fakeCandidateList);

        // act & assert

        var ex = Assert.Throws<CacheFullException>(() => cache.Put(2, "value2"));

        Assert.Equal("Cache is full (capacity: 1) and eviction failed for key: 2", ex.Message);
        _loggerMock.VerifyLog(LogLevel.Error, $"Eviction candidate ({fakeCandidate}) was not found in the cache", Times.Once());
    }
}
