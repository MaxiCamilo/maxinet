using System;
using System.Collections.Concurrent;

namespace MaxiNet
{
    internal class SharedTaskPool
    {
        private readonly ConcurrentQueue<Action> _actionList = new();

        private readonly IStreamController<SharedTaskPool> _newActionQueueStream = StreamController<SharedTaskPool>.ThreadSafe();


        public bool HasPendingActions => !_actionList.IsEmpty;



        public Result<Nothing> QueueAction(Action a)
        {
            _actionList.Enqueue(a);

            _newActionQueueStream.AddItem(this);

            return Res.Ok;
        }

        public bool TryDequeue(out Action? action)
        {
            return _actionList.TryDequeue(out action);
        }

        public Result<Action> ParalyzeNextTask(CancellationToken? cancellationToken = null)
        {
            while (true)
            {
                if (TryDequeue(out var action))
                {
                    if (action != null)
                        return Res.Value(action);
                }

                if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
                    return Res.ValError<Action>(new Oration("Operation was cancelled"));

                if (_newActionQueueStream.ParalyzeWaitItem<SharedTaskPool>().OnError(out var error))
                {
                    return error.Cast<Action>();
                }
            }
        }



        public SynchronizationContext BuildSynchronizationContext() => new PoolContext(this);

        public Task<Result<T>> BuildTask<T>(Func<T> func)
        {
            var task = new MaxiAsyncTask<T>()
            {
                Action = async (_) =>
                {
                    try
                    {
                        return Res.Value(func());
                    }
                    catch (Exception ex)
                    {
                        return new ExceptionResult<T>(ex, new Oration("An exception occurred"));
                    }
                }
            };
            return task.Run((x) => QueueAction(x));
        }

        public Task<Result<Nothing>> BuildTask(Action func)
        {
            var task = new MaxiAsyncTask<Nothing>()
            {
                Action = async (_) =>
                {
                    try
                    {
                        return Res.Ok;
                    }
                    catch (Exception ex)
                    {
                        return new ExceptionResult<Nothing>(ex, new Oration("An exception occurred"));
                    }
                }
            };
            return task.Run((x) => QueueAction(x));
        }

        public Task<Result<T>> BuildTask<T>(Func<Task<T>> func)
        {
            var task = new MaxiAsyncTask<T>()
            {
                Action = async (ct) =>
                {
                    try
                    {
                        var value = Task.Run(() => func(), ct);
                        return Res.Value(await value);
                    }
                    catch (Exception ex)
                    {
                        return new ExceptionResult<T>(ex, new Oration("An exception occurred"));
                    }
                }
            };
            return task.Run((x) => QueueAction(x));
        }

        public Task<Result<T>> BuildTask<T>(Func<Task<Result<T>>> func)
        {
            var task = new MaxiAsyncTask<T>()
            {
                Action = async (ct) =>
                {
                    try
                    {
                        var value = Task.Run(() => func(), ct);
                        return await value;
                    }
                    catch (Exception ex)
                    {
                        return new ExceptionResult<T>(ex, new Oration("An exception occurred"));
                    }
                }
            };
            return task.Run((x) => QueueAction(x));
        }

        private sealed class PoolContext(SharedTaskPool pool) : SynchronizationContext
        {
            public override void Post(SendOrPostCallback d, object? s)
                => pool.QueueAction(() => d(s));

            public override SynchronizationContext CreateCopy() => this;
        }
    }
}
