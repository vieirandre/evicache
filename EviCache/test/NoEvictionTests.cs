﻿using EviCache.Enums;
using EviCache.Tests.Helpers;

namespace EviCache.Tests;

public class NoEvictionTests : CacheTestsBase
{
    protected override EvictionPolicy EvictionPolicy => EvictionPolicy.NoEviction;
    protected override bool SupportsEviction => false;

    [Fact]
    public void Should_FailSilently_WhenAddingNewItem_OverCapacity_WithPut()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        cache.Put(3, "value3");

        // assert

        Assert.False(cache.TryGet(3, out _));
        Assert.False(cache.Count > cache.Capacity);
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public void Should_FailSilently_WhenAddingNewItem_OverCapacity_WithGetOrAdd()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        _ = cache.GetOrAdd(3, "value3");

        // assert

        Assert.False(cache.TryGet(3, out _));
        Assert.False(cache.Count > cache.Capacity);
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public void Should_FailSilently_WhenAddingNewItem_OverCapacity_WithAddOrUpdate()
    {
        // arrange

        var cache = CreateCache<int, string>(2, _loggerMock.Object);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        _ = cache.AddOrUpdate(3, "value3");

        // assert

        Assert.False(cache.TryGet(3, out _));
        Assert.False(cache.Count > cache.Capacity);
        _loggerMock.VerifyNoFailureLogsWereCalledInEviction();
    }

    [Fact]
    public void Should_UpdateExistingKey_WhenCacheIsFull_WithPut()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        cache.Put(1, "updatedValue1");

        // assert

        Assert.Equal("updatedValue1", cache.Get(1));
    }

    [Fact]
    public void Should_ReturnOriginalValue_WhenCacheIsFull_WithGetOrAdd()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        string originalValue = cache.GetOrAdd(1, "updatedValue1");

        // assert

        Assert.Equal("value1", originalValue);
        Assert.Equal("value1", cache.Get(1));
    }

    [Fact]
    public void Should_UpdateExistingKey_WhenCacheIsFull_WithAddOrUpdate()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        string updatedValue = cache.AddOrUpdate(1, "updatedValue1");

        // assert

        Assert.Equal("updatedValue1", updatedValue);
        Assert.Equal("updatedValue1", cache.Get(1));
    }

    [Fact]
    public void Should_Succeed_WhenNewItemAddedAfterRemoval()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act & assert

        bool removed = cache.Remove(1);
        Assert.True(removed);

        cache.Put(3, "value3");

        // assert

        Assert.True(cache.TryGet(3, out var value3));
        Assert.Equal("value3", value3);
    }

    [Fact]
    public void Should_AcceptNewItems_AfterClear()
    {
        // arrange

        var cache = CreateCache<int, string>(2);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        cache.Clear();
        cache.Put(3, "value3");

        // assert

        Assert.True(cache.TryGet(3, out var value3));
        Assert.Equal("value3", value3);
    }
}