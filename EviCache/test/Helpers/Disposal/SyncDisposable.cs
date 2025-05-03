namespace EviCache.Tests.Helpers.Disposal;

public class SyncDisposable : IDisposable
{
    public bool IsDisposed { get; private set; }
    private readonly TaskCompletionSource<bool> _tcs = new();

    public Task DisposalTask => _tcs.Task;

    public void Dispose()
    {
        IsDisposed = true;
        _tcs.TrySetResult(true);
    }
}