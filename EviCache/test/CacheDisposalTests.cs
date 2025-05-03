using EviCache.Tests.Helpers;
using EviCache.Tests.Helpers.Disposal;
using Microsoft.Extensions.Logging;
using Moq;

namespace EviCache.Tests;

public abstract partial class CacheTestsBase
{
    [Fact]
    public async Task Should_DisposeSyncDisposableItems_WhenClearIsCalled()
    {
        // arrange

        var cache = CreateCache<int, SyncDisposable>(3, _loggerMock.Object);

        var item1 = new SyncDisposable();
        var item2 = new SyncDisposable();

        cache.Put(1, item1);
        cache.Put(2, item2);

        // act

        cache.Clear();

        // assert

        Assert.Equal(0, cache.Count);

        await Task.WhenAll(item1.DisposalTask, item2.DisposalTask);
        Assert.True(item1.IsDisposed);
        Assert.True(item2.IsDisposed);

        _loggerMock.VerifyLog(LogLevel.Error, "Error while disposing cache item in the background", Times.Never());
    }

    [Fact]
    public async Task Should_DisposeAsyncDisposableItems_WhenClearIsCalled()
    {
        // arrange

        var cache = CreateCache<int, AsyncDisposable>(3, _loggerMock.Object);

        var item1 = new AsyncDisposable();
        var item2 = new AsyncDisposable();

        cache.Put(1, item1);
        cache.Put(2, item2);

        // act

        cache.Clear();

        // assert

        Assert.Equal(0, cache.Count);

        await Task.WhenAll(item1.DisposalTask, item2.DisposalTask);
        Assert.True(item1.IsAsyncDisposed);
        Assert.True(item2.IsAsyncDisposed);

        _loggerMock.VerifyLog(LogLevel.Error, "Error while disposing cache item in the background", Times.Never());
    }

    [Fact]
    public void Should_DisposeSyncDisposableItem_WhenRemoveIsCalled()
    {
        // arrange

        var cache = CreateCache<int, SyncDisposable>(3, _loggerMock.Object);

        var item = new SyncDisposable();
        cache.Put(1, item);

        // act

        bool removed = cache.Remove(1);

        // assert

        Assert.True(removed);
        Assert.True(item.IsDisposed);
        _loggerMock.VerifyLog(LogLevel.Error, "Error while disposing cache item in the background", Times.Never());
    }

    [Fact]
    public void Should_DisposeAsyncDisposableItem_WhenRemoveIsCalled()
    {
        // arrange

        var cache = CreateCache<int, AsyncDisposable>(3, _loggerMock.Object);

        var item = new AsyncDisposable();
        cache.Put(1, item);

        // act

        bool removed = cache.Remove(1);

        // assert

        Assert.True(removed);
        Assert.True(item.IsAsyncDisposed);
        _loggerMock.VerifyLog(LogLevel.Error, "Error while disposing cache item in the background", Times.Never());
    }

    [Fact]
    public async Task Should_DisposeItems_WhenCacheIsDisposed()
    {
        // arrange

        var item1 = new SyncDisposable();
        var item2 = new AsyncDisposable();

        using (var cache = CreateCache<int, object>(3, _loggerMock.Object))
        {
            cache.Put(1, item1);
            cache.Put(2, item2);

            // act: dispose called when exiting using block
        }

        // assert

        await Task.WhenAll(item1.DisposalTask, item2.DisposalTask);
        Assert.True(item1.IsDisposed);
        Assert.True(item2.IsAsyncDisposed);

        _loggerMock.VerifyLog(LogLevel.Error, "Error while disposing cache item in the background", Times.Never());
    }

    [Fact]
    public void Should_NotAttemptToDisposeNonDisposableItems_WhenClearIsCalled()
    {
        // arrange

        var cache = CreateCache<int, string>(3, _loggerMock.Object);

        cache.Put(1, "value1");
        cache.Put(2, "value2");

        // act

        cache.Clear();

        // assert

        Assert.Equal(0, cache.Count);
        _loggerMock.VerifyLog(LogLevel.Error, "Error while disposing cache item in the background", Times.Never());
    }
}
