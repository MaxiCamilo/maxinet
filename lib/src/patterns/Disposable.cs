using System;

namespace MaxiNet
{
    public interface IMaxiDisposable : IDisposable
    {
        public bool IsDisposed { get; }

    }


    abstract public class Disposable : IDisposable, IMaxiDisposable
    {

        public bool IsDisposed { get; private set; } = false;

        protected virtual void PerformDispose() { }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            IsDisposed = true;
            PerformDispose();

        }
    }

    static public class DisposableExtensions
    {
        static public Result<Nothing> ErrorIfDispose(this IMaxiDisposable disposable)
        {
            if (disposable.IsDisposed)
            {
                return Res.Error("The object you are trying to use has been disposed and cannot (and should not) be reused");
            }
            return Res.Ok;
        }
    }




}
