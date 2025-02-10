using EviCache.Tests.Utils;

namespace EviCache.Tests;

public class EviCacheTests
{
    [Fact]
    public void Should_ReturnValue_WhenKeyIsInsertedAndRetrieved()
    {
        // arrange

        var cache = new EviCache<int, string>(3);

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

        var cache = new EviCache<int, string>(2);

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

        Assert.Throws<ArgumentOutOfRangeException>(() => new EviCache<int, string>(invalidCapacity));
    }

    [Fact]
    public void Should_StoreAndRetrieve_WhenKeyIsStringAndValueIsInt()
    {
        // arrange

        var cache = new EviCache<string, int>(2);

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

        var cache = new EviCache<int, string>(2);
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
    public void Should_NotDisposeItem_WhenTryGetIsCalled()
    {
        // arrange

        var cache = new EviCache<int, DisposableDummy>(2);
        var disposableItem = new DisposableDummy();
        cache.Put(1, disposableItem);

        // act

        var result = cache.TryGet(1, out var disposableReturn);

        // assert

        Assert.True(result);
        Assert.False(disposableItem.IsDisposed);
        Assert.Equal(disposableItem, disposableReturn);
    }

    [Fact]
    public void Should_NotExceedCapacity_WhenCacheIsFull()
    {
        // arrange

        int capacity = 2;
        var cache = new EviCache<int, string>(capacity);

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

        var cache = new EviCache<int, DisposableDummy>(2);
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

        var cache = new EviCache<int, DisposableDummy>(2);
        var disposableItem = new DisposableDummy();

        cache.Put(1, disposableItem);

        // act

        bool removed = cache.Remove(9);

        // assert

        Assert.False(removed);
        Assert.False(disposableItem.IsDisposed);
    }

    [Fact]
    public void Should_ClearAllDisposableItems()
    {
        // arrange

        var cache = new EviCache<int, DisposableDummy>(3);
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
        Assert.False(cache.TryGet(1, out _));
        Assert.False(cache.TryGet(2, out _));
        Assert.False(cache.TryGet(3, out _));

        // obsolete; Clear's disposal now performed in the background
        //Assert.True(item1.IsDisposed);
        //Assert.True(item2.IsDisposed);
        //Assert.True(item3.IsDisposed);
    }

    [Fact]
    public void Should_ClearAllItems_WhenClearIsCalled()
    {
        // arrange

        var cache = new EviCache<int, string>(3);

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

        var cache = new EviCache<int, string>(2);

        // act

        var result = cache.GetOrAdd(1, "value1");

        // assert

        Assert.Equal("value1", result);
        Assert.True(cache.TryGet(1, out var storedValue));
        Assert.Equal("value1", storedValue);
    }

    [Fact]
    public void Should_ReturnExistingValueAndMoveToFront_WhenKeyExistsInGetOrAdd()
    {
        // arrange

        var cache = new EviCache<int, string>(2);
        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        var result = cache.GetOrAdd(1, "newValue1");

        // assert

        Assert.Equal("value1", result);
        var keys = cache.GetKeysInOrder();
        Assert.Equal([1, 2], keys);
    }

    [Fact]
    public void Should_EvictLeastRecentlyUsed_WhenAddingNewItemExceedsCapacityInGetOrAdd()
    {
        // arrange

        var cache = new EviCache<int, string>(2);
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
    }

    [Fact]
    public void Should_AddNewValueAndNotEvict_WhenUnderCapacityInGetOrAdd()
    {
        // arrange

        var cache = new EviCache<int, string>(3);
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

        var cache = new EviCache<int, DisposableDummy>(2);

        // act

        var disposable1 = cache.GetOrAdd(1, new DisposableDummy());

        // assert

        Assert.NotNull(disposable1);
        Assert.False(disposable1.IsDisposed);
        Assert.True(cache.TryGet(1, out var retrieved));
        Assert.Equal(disposable1, retrieved);
    }

    [Fact]
    public void Should_DisposeEvictedItem_WhenAddingNewItemExceedsCapacityWithDisposableValue()
    {
        // arrange

        var cache = new EviCache<int, DisposableDummy>(2);
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
    }

    [Fact]
    public void Should_ReturnKeysInMruToLruOrder_WhenItemsAreAdded()
    {
        // arrange

        var cache = new EviCache<int, string>(3);
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

        var cache = new EviCache<int, string>(3);
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

        var cache = new EviCache<int, string>(2);
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

        var cache = new EviCache<int, string>(3);
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

        var cache = new EviCache<int, string>(3);
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

    [Fact]
    public void Should_NotDuplicateKeysInOrder_WhenAddingExistingKey()
    {
        // arrange

        var cache = new EviCache<int, string>(3);
        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        cache.Put(2, "newValue2");
        var keys = cache.GetKeysInOrder();

        // assert

        Assert.Equal([2, 3, 1], keys);
        Assert.Equal("newValue2", cache.TryGet(2, out var value2) ? value2 : null);
    }

    [Fact]
    public void Should_HandleGetKeysInOrder_WithDisposableItems()
    {
        // arrange

        var cache = new EviCache<int, DisposableDummy>(3);
        var disposable1 = new DisposableDummy();
        var disposable2 = new DisposableDummy();
        var disposable3 = new DisposableDummy();

        cache.Put(1, disposable1);
        cache.Put(2, disposable2);
        cache.Put(3, disposable3);

        // act

        cache.TryGet(1, out var _);
        var keys = cache.GetKeysInOrder();

        // assert

        Assert.Equal([1, 3, 2], keys);
        Assert.False(disposable1.IsDisposed);
    }

    [Fact]
    public void Should_ReturnValue_WhenKeyExists()
    {
        // arrange

        var cache = new EviCache<int, string>(3);
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

        var cache = new EviCache<int, string>(2);
        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act & assert

        var exception = Assert.Throws<KeyNotFoundException>(() => cache.Get(3));
        Assert.Equal("The key '3' wasn't found in the cache", exception.Message);
    }

    [Fact]
    public void Should_MoveKeyToFront_WhenGetIsCalled()
    {
        // arrange

        var cache = new EviCache<int, string>(3);
        cache.Put(1, "value1");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        var result = cache.Get(1);

        // assert

        Assert.Equal("value1", result);
        var keys = cache.GetKeysInOrder();
        Assert.Equal([1, 3, 2], keys);
    }

    [Fact]
    public void Should_NotDisposeItem_WhenGetIsCalled()
    {
        // arrange

        var cache = new EviCache<int, DisposableDummy>(2);
        var disposableItem = new DisposableDummy();
        cache.Put(1, disposableItem);

        // act

        var result = cache.Get(1);

        // assert

        Assert.False(disposableItem.IsDisposed);
        Assert.Equal(disposableItem, result);
    }

    [Fact]
    public void Should_TrackHitsMissesAndEvictions_WhenCacheCapacityIs15()
    {
        // arrange

        int capacity = 15;
        var cache = new EviCache<int, string>(capacity);

        for (int i = 1; i <= capacity; i++)
        {
            cache.Put(i, $"value{i}");
        }

        // act & assert

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

        // assert

        Assert.Equal(15, cache.Count);

        Assert.Equal(10, cache.Hits);
        Assert.Equal(5, cache.Misses);
        Assert.Equal(5, cache.Evictions);
    }

    [Fact]
    public void Should_ReturnOrderedSnapshot_WhenCacheContains15Items()
    {
        // arrange

        int capacity = 15;
        var cache = new EviCache<int, string>(capacity);

        for (int i = 1; i <= capacity; i++)
        {
            cache.Put(i, $"value{i}");
        }

        // act

        var snapshot = cache.GetSnapshot();

        // assert

        Assert.Equal(capacity, snapshot.Count);

        Assert.Equal(15, snapshot[0].Key);
        Assert.Equal("value15", snapshot[0].Value);
        Assert.Equal(1, snapshot[capacity - 1].Key);
        Assert.Equal("value1", snapshot[capacity - 1].Value);
    }

    [Fact]
    public void Should_UpdateExistingKey_AndReturnNewValue()
    {
        // arrange

        var cache = new EviCache<int, string>(5);
        cache.Put(1, "oldValue");
        cache.Put(2, "value2");
        cache.Put(3, "value3");

        // act

        var result = cache.AddOrUpdate(1, "newValue");
        cache.Get(3);

        // assert

        Assert.Equal("newValue", result);
        Assert.Equal("newValue", cache.Get(1));

        var keys = cache.GetKeysInOrder();
        Assert.Equal(1, keys[0]);
    }

    [Fact]
    public void Should_AddNewKey_AndReturnValue()
    {
        // arrange

        var cache = new EviCache<int, string>(5);

        // act

        var result = cache.AddOrUpdate(2, "value2");

        // assert

        Assert.Equal("value2", result);
        Assert.Equal("value2", cache.Get(2));
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void Should_EvictLeastRecentlyUsed_WhenAddingNewKeyExceedingCapacity()
    {
        // arrange

        var cache = new EviCache<int, string>(3);
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
    }

    [Fact]
    public void Should_UpdateMetrics_OnAddOrUpdate_ForExistingAndNewKeys()
    {
        // arrange

        var cache = new EviCache<int, string>(5);

        // act & assert:

        cache.AddOrUpdate(1, "value1");
        Assert.Equal(1, cache.Misses);
        Assert.Equal(0, cache.Hits);

        cache.AddOrUpdate(1, "value1Updated");
        Assert.Equal(1, cache.Hits);

        cache.AddOrUpdate(2, "value2");
        Assert.Equal(2, cache.Misses);
    }
}