using EviCache.Enums;
using EviCache.Tests.Utils;
using Microsoft.Extensions.Logging;
using Moq;

namespace EviCache.Tests;

public class LfuTests : CacheTestsBase
{
    private const EvictionPolicy _evictionPolicy = EvictionPolicy.LFU;

    protected override EvictionPolicy EvictionPolicy => _evictionPolicy;

    [Fact]
    public void Should_EvictLeastFrequentlyUsed_WhenCapacityIsExceeded()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        cache.Get(1);
        cache.Get(1);

        // act

        cache.Put(3, "value3");

        // assert

        Assert.False(cache.TryGet(2, out _));
        Assert.True(cache.TryGet(1, out var value1));
        Assert.Equal("value1", value1);
        Assert.True(cache.TryGet(3, out var value3));
        Assert.Equal("value3", value3);

        _loggerMock.VerifyLog(LogLevel.Debug, $"Evicted key from cache: 2 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public void Should_UpdateExistingKeyAndEvictLeastFrequentlyUsed_WhenReinsertingKeyOverCapacity()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        cache.Put(1, "oldValue");
        cache.Put(2, "value2");

        // act

        cache.Put(1, "newValue");
        cache.Put(3, "value3");

        // assert

        Assert.False(cache.TryGet(2, out _));
        Assert.Equal("newValue", cache.Get(1));
        Assert.Equal("value3", cache.Get(3));

        _loggerMock.VerifyLog(LogLevel.Debug, $"Evicted key from cache: 2 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public void Should_EvictFirstInsertedKey_WhenFrequenciesAreEqual()
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

        _loggerMock.VerifyLog(LogLevel.Debug, $"Evicted key from cache: 1 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public void Should_EvictCorrectly_AfterMultipleAccesses()
    {
        // arrange

        var cache = CreateCache<int, string>(3, _loggerMock.Object);

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3"); // freq = 1

        // act

        cache.Get(1); // freq = 2
        cache.Get(1); // freq = 3
        cache.Get(1);
        cache.Get(2); // freq = 2
        cache.Get(2);

        cache.Put(4, "value4"); // evict

        // assert

        Assert.False(cache.TryGet(3, out _));
        Assert.True(cache.TryGet(1, out string value1));
        Assert.Equal("value1", value1);
        Assert.True(cache.TryGet(2, out string value2));
        Assert.Equal("value2", value2);
        Assert.True(cache.TryGet(4, out string value4));
        Assert.Equal("value4", value4);

        _loggerMock.VerifyLog(LogLevel.Debug, $"Evicted key from cache: 3 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public void Should_EvictLeastFrequentlyUsed_MultipleTimes()
    {
        // arrange

        var cache = CreateCache<int, string>(3, _loggerMock.Object);

        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        cache.Get(1);
        cache.Get(1);
        cache.Get(2);

        // act

        cache.Put(4, "value4");

        // assert

        Assert.False(cache.TryGet(3, out _));
        Assert.True(cache.TryGet(1, out _));
        Assert.True(cache.TryGet(2, out _));
        Assert.True(cache.TryGet(4, out _));
        Assert.Equal(1, cache.Evictions);

        // act

        cache.Put(5, "value5");
        cache.Put(6, "value6");

        // assert

        Assert.Equal(3, cache.Count);
        Assert.Equal(3, cache.Evictions);

        _loggerMock.VerifyLog(LogLevel.Debug, $"Evicted key from cache: 3 | Total evictions: 1", Times.Once());
        _loggerMock.VerifyLog(LogLevel.Debug, $"Evicted key from cache: 4 | Total evictions: 2", Times.Once());
        _loggerMock.VerifyLog(LogLevel.Debug, $"Evicted key from cache: 5 | Total evictions: 3", Times.Once());
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }
}
