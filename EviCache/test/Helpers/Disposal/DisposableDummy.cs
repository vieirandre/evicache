namespace EviCache.Tests.Helpers.Disposal;

public class DisposableDummy : IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
    }
}