namespace EviCache.Extensions;

internal static class SemaphoreExtensions
{
    internal static IDisposable Lock(this SemaphoreSlim semaphore)
    {
        semaphore.Wait();
        return new Releaser(semaphore);
    }

    internal static async ValueTask<IAsyncDisposable> LockAsync(this SemaphoreSlim semaphore, CancellationToken ct)
    {
        await semaphore.WaitAsync(ct).ConfigureAwait(false);
        return new AsyncReleaser(semaphore);
    }

    private readonly struct Releaser : IDisposable, IAsyncDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        public Releaser(SemaphoreSlim semaphore) => _semaphore = semaphore;
        public void Dispose() => _semaphore.Release();
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private readonly struct AsyncReleaser : IAsyncDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        public AsyncReleaser(SemaphoreSlim semaphore) => _semaphore = semaphore;
        public ValueTask DisposeAsync()
        {
            _semaphore.Release();
            return ValueTask.CompletedTask;
        }
    }
}
