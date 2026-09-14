namespace MaxiNet;

public interface IMaxiDisposable : IDisposable
{
    public bool IsDisposed { get; }
}

public abstract class Disposable : IDisposable, IMaxiDisposable
{
    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
        PerformDispose();
    }

    public bool IsDisposed { get; private set; }

    protected virtual void PerformDispose()
    {
    }
}

public static class DisposableExtensions
{
    public static Result<Nothing> ErrorIfDispose(this IMaxiDisposable disposable)
    {
        if (disposable.IsDisposed)
            return Res.Error(
                "The object you are trying to use has been disposed and cannot (and should not) be reused");
        return Res.Ok;
    }
}