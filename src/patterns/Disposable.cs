using System;

namespace MaxiNet
{
    abstract public class Disposable : IDisposable
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
}
