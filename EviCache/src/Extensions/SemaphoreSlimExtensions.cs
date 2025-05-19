namespace EviCache.Extensions;

internal static class SemaphoreSlimExtensions
{
    internal static void Execute(this SemaphoreSlim gate, Action body)
    {
        gate.Wait();
        try { body(); }
        finally { gate.Release(); }
    }

    internal static T Execute<T>(this SemaphoreSlim gate, Func<T> body)
    {
        gate.Wait();
        try { return body(); }
        finally { gate.Release(); }
    }

    internal static async Task ExecuteAsync(this SemaphoreSlim gate, Action body, CancellationToken ct = default)
    {
        await gate.WaitAsync(ct).ConfigureAwait(false);
        try { body(); }
        finally { gate.Release(); }
    }

    internal static async Task<T> ExecuteAsync<T>(this SemaphoreSlim gate, Func<T> body, CancellationToken ct = default)
    {
        await gate.WaitAsync(ct).ConfigureAwait(false);
        try { return body(); }
        finally { gate.Release(); }
    }
}