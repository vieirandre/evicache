namespace EviCache.Extensions;

public static class SemaphoreSlimExtensions
{
    public static void Execute(this SemaphoreSlim gate, Action body)
    {
        gate.Wait();
        try { body(); }
        finally { gate.Release(); }
    }

    public static T Execute<T>(this SemaphoreSlim gate, Func<T> body)
    {
        gate.Wait();
        try { return body(); }
        finally { gate.Release(); }
    }

    public static async Task ExecuteAsync(this SemaphoreSlim gate, Action body, CancellationToken ct = default)
    {
        await gate.WaitAsync(ct).ConfigureAwait(false);
        try { body(); }
        finally { gate.Release(); }
    }

    public static async Task<T> ExecuteAsync<T>(this SemaphoreSlim gate, Func<T> body, CancellationToken ct = default)
    {
        await gate.WaitAsync(ct).ConfigureAwait(false);
        try { return body(); }
        finally { gate.Release(); }
    }
}
