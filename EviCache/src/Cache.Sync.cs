namespace EviCache;

public sealed partial class Cache<TKey, TValue> where TKey : notnull
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    private void WithLock(Action body)
    {
        _gate.Wait();
        try { body(); }
        finally { _gate.Release(); }
    }

    private T WithLock<T>(Func<T> body)
    {
        _gate.Wait();
        try { return body(); }
        finally { _gate.Release(); }
    }

    private async Task WithLockAsync(Action body, CancellationToken ct)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try { body(); }
        finally { _gate.Release(); }
    }

    private async Task<T> WithLockAsync<T>(Func<T> body, CancellationToken ct)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try { return body(); }
        finally { _gate.Release(); }
    }
}
