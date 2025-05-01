using EviCache.Enums;
using EviCache.Exceptions;
using EviCache.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace EviCache.Tests;

public class LfuAsyncTests : CacheAsyncTestsBase
{
    protected override EvictionPolicy EvictionPolicy => EvictionPolicy.LFU;

    [Fact]
    public async Task Should_EvictLeastFrequentlyUsed_WhenCapacityIsExceeded()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        await cache.GetAsync(1);
        await cache.GetAsync(1);

        // act

        await cache.PutAsync(3, "value3");

        // assert

        Assert.False((await cache.TryGetAsync(2)).Found);

        var (Found1, Value1) = await cache.TryGetAsync(1);
        Assert.True(Found1);
        Assert.Equal("value1", Value1);

        var (Found3, Value3) = await cache.TryGetAsync(3);
        Assert.True(Found3);
        Assert.Equal("value3", Value3);

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 2 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public async Task Should_UpdateExistingKeyAndEvictLeastFrequentlyUsed_WhenReinsertingKeyOverCapacity()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        await cache.PutAsync(1, "oldValue");
        await cache.PutAsync(2, "value2");

        // act

        await cache.PutAsync(1, "newValue");
        await cache.PutAsync(3, "value3");

        // assert

        Assert.False((await cache.TryGetAsync(2)).Found);
        Assert.Equal("newValue", await cache.GetAsync(1));
        Assert.Equal("value3", await cache.GetAsync(3));

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 2 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public async Task Should_EvictFirstInsertedKey_WhenFrequenciesAreEqual()
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
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public async Task Should_EvictCorrectly_AfterMultipleAccesses()
    {
        // arrange

        var cache = CreateCache<int, string>(3, _loggerMock.Object);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3"); // freq = 1

        // act

        await cache.GetAsync(1); // freq = 2
        await cache.GetAsync(1); // freq = 3
        await cache.GetAsync(1);
        await cache.GetAsync(2); // freq = 2
        await cache.GetAsync(2);

        await cache.PutAsync(4, "value4"); // evict

        // assert

        Assert.False((await cache.TryGetAsync(3)).Found);

        var (Found1, Value1) = await cache.TryGetAsync(1);
        Assert.True(Found1);
        Assert.Equal("value1", Value1);

        var (Found2, Value2) = await cache.TryGetAsync(2);
        Assert.True(Found2);
        Assert.Equal("value2", Value2);

        var (Found4, Value4) = await cache.TryGetAsync(4);
        Assert.True(Found4);
        Assert.Equal("value4", Value4);

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 3 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public async Task Should_EvictLeastFrequentlyUsed_MultipleTimes()
    {
        // arrange

        var cache = CreateCache<int, string>(3, _loggerMock.Object);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");
        await cache.PutAsync(3, "value3");

        await cache.GetAsync(1);
        await cache.GetAsync(1);
        await cache.GetAsync(2);

        // act

        await cache.PutAsync(4, "value4");

        // assert

        Assert.False((await cache.TryGetAsync(3)).Found);
        Assert.True((await cache.TryGetAsync(1)).Found);
        Assert.True((await cache.TryGetAsync(2)).Found);
        Assert.True((await cache.TryGetAsync(4)).Found);
        Assert.Equal(1, cache.Evictions);

        // act

        await cache.PutAsync(5, "value5");
        await cache.PutAsync(6, "value6");

        // assert

        Assert.Equal(3, cache.Count);
        Assert.Equal(3, cache.Evictions);

        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 3 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 4 | Total evictions: 2", Times.Once());
        _loggerMock.VerifyLog(LogLevel.Debug, "Evicted key from cache: 5 | Total evictions: 3", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public async Task Should_ReturnSnapshotInOrderOfIncreasingFrequency()
    {
        // arrange

        int capacity = 20;
        var expectedOrder = Enumerable.Range(1, capacity);

        var cache = CreateCache<int, string>(capacity);

        for (int i = 1; i <= capacity; i++)
        {
            await cache.PutAsync(i, $"value{i}");
        }

        // act

        for (int i = 1; i <= capacity; i++)
        {
            for (int j = 1; j < i; j++)
            {
                await cache.GetAsync(i);
            }
        }

        // assert

        var actualOrder = cache.GetSnapshot().Select(kvp => kvp.Key);

        Assert.Equal(expectedOrder, actualOrder);
    }

    [Fact]
    public async Task Should_EvictLeastRecentlyInsertedKey_WhenMultipleKeysHaveSameLowestFrequency()
    {
        // arrange

        int capacity = 10;
        var cache = CreateCache<int, string>(capacity);

        for (int i = 1; i <= capacity; i++)
        {
            await cache.PutAsync(i, $"value{i}");
        }

        await cache.GetAsync(2);

        // act

        await cache.PutAsync(11, "value11");

        // assert

        Assert.Equal(capacity, cache.Count);
        Assert.False((await cache.TryGetAsync(1)).Found);

        var (Found11, Value11) = await cache.TryGetAsync(11);
        Assert.True(Found11);
        Assert.Equal("value11", Value11);
    }

    [Fact]
    public async Task Should_ThrowException_WhenEvictionFails_DueToNoCandidate()
    {
        // arrange

        var cache = CreateCache<int, string>(1, _loggerMock.Object);
        await cache.PutAsync(1, "value1");

        await cache.OverrideEvictionCandidateCollection(EvictionPolicy.GetEvictionCandidateCollectionFieldName(), new SortedDictionary<int, LinkedList<int>>());

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

        var fakeFrequencyBuckets = new SortedDictionary<int, LinkedList<int>>();
        var fakeCandidateList = new LinkedList<int>();
        int fakeCandidate = 999;
        fakeCandidateList.AddLast(fakeCandidate);
        fakeFrequencyBuckets.Add(1, fakeCandidateList);

        await cache.OverrideEvictionCandidateCollection(EvictionPolicy.GetEvictionCandidateCollectionFieldName(), fakeFrequencyBuckets);

        // act & assert

        var ex = await Assert.ThrowsAsync<CacheFullException>(async () => await cache.PutAsync(2, "value2"));

        Assert.Equal("Cache is full (capacity: 1). Failed to evict any item while adding key: 2", ex.Message);
        _loggerMock.VerifyLog(LogLevel.Error, $"Eviction candidate ({fakeCandidate}) was not found in the cache", Times.Once());
    }
}
