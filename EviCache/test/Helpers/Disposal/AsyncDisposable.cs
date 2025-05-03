namespace EviCache.Tests.Helpers.Disposal;

public class AsyncDisposable : IAsyncDisposable
{
    public bool IsAsyncDisposed { get; private set; }
    private readonly TaskCompletionSource<bool> _tcs = new();

    public Task DisposalTask => _tcs.Task;

    public async ValueTask DisposeAsync()
    {
        await Task.Delay(10);
        IsAsyncDisposed = true;
        _tcs.TrySetResult(true);
    }
}