using LruCache.Tests.Utils;

namespace LruCache.Tests;

public class LruCacheTests
{
    [Fact]
    public void Should_ReturnValue_WhenKeyIsInsertedAndRetrieved()
    {
        // arrange

        var cache = new LruCache<int, string>(3);

        // act

        cache.Put(1, "value");
        cache.TryGet(1, out var result);

        // assert

        Assert.Equal("value", result);
    }

    [Fact]
    public void Should_EvictLeastRecentlyUsed_WhenCapacityIsExceeded()
    {
        // arrange

        var cache = new LruCache<int, string>(2);

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
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Should_ThrowArgumentOutOfRangeException_WhenCapacityIsZeroOrNegative(int invalidCapacity)
    {
        // act & assert

        Assert.Throws<ArgumentOutOfRangeException>(() => new LruCache<int, string>(invalidCapacity));
    }

    [Fact]
    public void Should_StoreAndRetrieve_WhenKeyIsStringAndValueIsInt()
    {
        // arrange

        var cache = new LruCache<string, int>(2);

        // act

        cache.Put("this", 10);
        cache.Put("that", 20);
        cache.TryGet("that", out var thatValue);

        // assert

        Assert.Equal(20, thatValue);
    }

    [Fact]
    public void Should_UpdateExistingKeyAndEvictLeastRecentlyUsed_WhenReinsertingKeyOverCapacity()
    {
        // arrange

        var cache = new LruCache<int, string>(2);
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
    }

    [Fact]
    public void Should_NotExceedCapacity_WhenCacheIsFull()
    {
        // arrange

        int capacity = 2;
        var cache = new LruCache<int, string>(capacity);

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

        var cache = new LruCache<int, DisposableDummy>(2);
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

        var cache = new LruCache<int, DisposableDummy>(2);
        var disposableItem = new DisposableDummy();

        cache.Put(1, disposableItem);

        // act

        bool removed = cache.Remove(9);

        // assert

        Assert.False(removed);
        Assert.False(disposableItem.IsDisposed);
    }

    [Fact]
    public void Should_ClearAllItemsAndDisposeThem()
    {
        // arrange

        var cache = new LruCache<int, DisposableDummy>(3);
        var item1 = new DisposableDummy();
        var item2 = new DisposableDummy();
        var item3 = new DisposableDummy();

        cache.Put(1, item1);
        cache.Put(2, item2);
        cache.Put(3, item3);

        // act

        cache.Clear();

        // assert

        Assert.Equal(0, cache.Count);
        Assert.True(item1.IsDisposed);
        Assert.True(item2.IsDisposed);
        Assert.True(item3.IsDisposed);
        Assert.False(cache.TryGet(1, out _));
        Assert.False(cache.TryGet(2, out _));
        Assert.False(cache.TryGet(3, out _));
    }

    [Fact]
    public void Should_ClearAllItemsWithoutDisposing_WhenValuesAreNotDisposable()
    {
        // arrange

        var cache = new LruCache<int, string>(3);

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

    [Fact]
    public void Should_AddNewValue_WhenKeyDoesNotExistInGetOrAdd()
    {
        // arrange

        var cache = new LruCache<int, string>(2);

        // act

        var result = cache.GetOrAdd(1, "value1");

        // assert

        Assert.Equal("value1", result);
        Assert.True(cache.TryGet(1, out var storedValue));
        Assert.Equal("value1", storedValue);
    }

    [Fact]
    public void Should_ReturnKeysInMruToLruOrder_WhenItemsAreAdded()
    {
        // arrange

        var cache = new LruCache<int, string>(3);
        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        var keys = cache.GetKeysInOrder();

        // assert

        Assert.Equal([3, 2, 1], keys);
    }

    [Fact]
    public void Should_UpdateKeyOrder_WhenItemsAreAccessed()
    {
        // arrange

        var cache = new LruCache<int, string>(3);
        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        cache.TryGet(1, out var _);
        var keys = cache.GetKeysInOrder();

        // assert

        Assert.Equal([1, 3, 2], keys);
    }

    [Fact]
    public void Should_RemoveEvictedKeyFromOrder_WhenCapacityIsExceeded()
    {
        // arrange

        var cache = new LruCache<int, string>(2);
        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        cache.Put(3, "value3");
        var keys = cache.GetKeysInOrder();

        // assert

        Assert.Equal([3, 2], keys);
        Assert.False(cache.TryGet(1, out _));
    }

    [Fact]
    public void Should_RemoveKeyFromOrder_WhenItemIsRemoved()
    {
        // arrange

        var cache = new LruCache<int, string>(3);
        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        cache.Remove(2);
        var keys = cache.GetKeysInOrder();

        // assert

        Assert.Equal([3, 1], keys);
        Assert.False(cache.TryGet(2, out _));
    }

    [Fact]
    public void Should_ReturnEmptyList_WhenCacheIsCleared()
    {
        // arrange

        var cache = new LruCache<int, string>(3);
        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        cache.Clear();
        var keys = cache.GetKeysInOrder();

        // assert

        Assert.Empty(keys);
        Assert.Equal(0, cache.Count);
    }
}