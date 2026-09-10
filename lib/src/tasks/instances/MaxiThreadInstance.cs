using System;

namespace MaxiNet
{
    internal class MaxiThreadInstance : Disposable
    {
        public required int Identifier { get; init; }
        public required SharedTaskPool TaskPool { get; init; }
        public required String Name { get; init; }


        private bool _threadStart = false;
        private Thread? _thread;

        private CancellationTokenSource? _cancellationToken;

        public Result<Nothing> StartThread()
        {
            if (_threadStart)
                return Res.ValError<Nothing>(new Oration("Thread already started"));

            _threadStart = true;
            _thread = new Thread(Loop)
            {
                IsBackground = true,
                Name = $"{Name}-{Identifier}"
            };


            _thread.Start();
            return Res.Ok;
        }

        private void Loop()
        {
            var synchronizationContext = TaskPool.BuildSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            _cancellationToken = new CancellationTokenSource();

            while (true)
            {
                if (IsDisposed)
                {
                    break;
                }
                else if (TaskPool.ParalyzeNextTask(_cancellationToken.Token).TryGetValue(out var action, out var error))
                {
                    action();
                }
                else
                {
                    Console.WriteLine(error);
                    break;
                }
            }



            Dispose();
        }

        protected override void PerformDispose()
        {
            base.PerformDispose();
            _thread?.Interrupt();
            _thread = null;
            _threadStart = false;
            _cancellationToken?.Cancel();
            _cancellationToken?.Dispose();
            _cancellationToken = null;

        }



    }
}
