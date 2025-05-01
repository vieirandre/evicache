using EviCache.Enums;
using EviCache.Exceptions;
using EviCache.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace EviCache.Tests;

public class NoEvictionAsyncTests : CacheAsyncTestsBase
{
    protected override EvictionPolicy EvictionPolicy => EvictionPolicy.NoEviction;
    protected override bool SupportsEviction => false;

    [Fact]
    public async Task Should_ThrowException_WhenAddingNewItem_OverCapacity_WithPut()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        // act & assert

        var exception = await Assert.ThrowsAsync<CacheFullException>(async () => await cache.PutAsync(3, "value3"));

        // assert

        Assert.False((await cache.TryGetAsync(3)).Found);
        Assert.False(cache.Count > cache.Capacity);
        Assert.Equal($"Cache is full (capacity: 2) and uses {EvictionPolicy} policy", exception.Message);
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public async Task Should_ThrowException_WhenAddingNewItem_OverCapacity_WithGetOrAdd()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        // act & assert

        var exception = await Assert.ThrowsAsync<CacheFullException>(async () => await cache.GetOrAddAsync(3, "value3"));

        // assert

        Assert.False((await cache.TryGetAsync(3)).Found);
        Assert.False(cache.Count > cache.Capacity);
        Assert.Equal($"Cache is full (capacity: 2) and uses {EvictionPolicy} policy", exception.Message);
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public async Task Should_ThrowException_WhenAddingNewItem_OverCapacity_WithAddOrUpdate()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        // act

        var exception = await Assert.ThrowsAsync<CacheFullException>(async () => await cache.AddOrUpdateAsync(3, "value3"));

        // assert

        Assert.False((await cache.TryGetAsync(3)).Found);
        Assert.False(cache.Count > cache.Capacity);
        Assert.Equal($"Cache is full (capacity: 2) and uses {EvictionPolicy} policy", exception.Message);
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public async Task Should_PopulateExceptionDataDictionary_WhenCacheIsFull()
    {
        // arrange

        var cache = CreateCache<int, string>(1);
        await cache.PutAsync(1, "value1");

        // act

        var ex = await Assert.ThrowsAsync<CacheFullException>(async () => await cache.PutAsync(2, "value2"));

        // assert

        Assert.True(ex.Data.Contains("Capacity"));
        Assert.Equal(1, ex.Data["Capacity"]);
        Assert.Equal("2", ex.Data["AttemptedKey"]);
        Assert.Equal("NoEviction", ex.Data["EvictionPolicy"]);
    }

    [Fact]
    public async Task Should_UpdateExistingKey_WhenCacheIsFull_WithPut()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        // act

        await cache.PutAsync(1, "updatedValue1");

        // assert

        Assert.Equal("updatedValue1", await cache.GetAsync(1));
    }

    [Fact]
    public async Task Should_ReturnOriginalValue_WhenCacheIsFull_WithGetOrAdd()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        // act

        string originalValue = await cache.GetOrAddAsync(1, "updatedValue1");

        // assert

        Assert.Equal("value1", originalValue);
        Assert.Equal("value1", await cache.GetAsync(1));
    }

    [Fact]
    public async Task Should_UpdateExistingKey_WhenCacheIsFull_WithAddOrUpdate()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        // act

        string updatedValue = await cache.AddOrUpdateAsync(1, "updatedValue1");

        // assert

        Assert.Equal("updatedValue1", updatedValue);
        Assert.Equal("updatedValue1", await cache.GetAsync(1));
    }

    [Fact]
    public async Task Should_Succeed_WhenNewItemAddedAfterRemoval()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        // act & assert

        bool removed = await cache.RemoveAsync(1);
        Assert.True(removed);

        await cache.PutAsync(3, "value3");

        // assert

        var (Found, Value) = await cache.TryGetAsync(3);
        Assert.True(Found);
        Assert.Equal("value3", Value);
    }

    [Fact]
    public async Task Should_AcceptNewItems_AfterClear()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        await cache.PutAsync(1, "value1");
        await cache.PutAsync(2, "value2");

        // act

        await cache.ClearAsync();
        await cache.PutAsync(3, "value3");

        // assert

        var (Found, Value) = await cache.TryGetAsync(3);
        Assert.True(Found);
        Assert.Equal("value3", Value);

        _loggerMock.VerifyLog(LogLevel.Information, "Cache cleared. Removed 2 items", Times.Once());
    }
}