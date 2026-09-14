using System.Collections.Concurrent;

namespace MaxiNet;

internal class SharedTaskPool
{
    private readonly IStreamController<SharedTaskPool> _newActionQueueStream =
        StreamController<SharedTaskPool>.ThreadSafe();
    //private readonly ConcurrentQueue<IMaxiAsyncTask> _taskList = new();

    private readonly ConcurrentQueue<Action> _taskList = new();


    public bool HasPendingActions => !_taskList.IsEmpty;


    public Result<Nothing> QueueAction(Action a)
    {
        _taskList.Enqueue(a);

        _newActionQueueStream.AddItem(this);

        return Res.Ok;
    }

    public bool TryDequeue(out Action? task)
    {
        return _taskList.TryDequeue(out task);
    }

    public Result<Action> ParalyzeNextAction(CancellationToken? cancellationToken = null)
    {
        while (true)
        {
            if (TryDequeue(out var task))
                if (task != null)
                    return Res.Value(task);

            if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
                return Res.ValError<Action>(new Oration("Operation was cancelled"));

            if (_newActionQueueStream.ParalyzeWaitItem<SharedTaskPool>().OnError(out var error))
                return error.Cast<Action>();
        }
    }


    public SynchronizationContext BuildSynchronizationContext()
    {
        return new PoolContext(this);
    }

    public Task<Result<T>> BuildTask<T>(Func<Result<T>> func)
    {
        var task = new MaxiAsyncTask<T>
        {
            Action = async _ =>
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    return new ExceptionResult<T>(ex, new Oration("An exception occurred"));
                }
            }
        };
        return QueueAction(() => task.BuildRunner()).OnError(out var queueError)
            ? Task.FromResult(queueError.Cast<T>())
            : task.WaitResult();
    }

    public Task<Result<T>> BuildTask<T>(Func<Task<Result<T>>> func)
    {
        var task = new MaxiAsyncTask<T>
        {
            Action = async _ =>
            {
                try
                {
                    return await func();
                }
                catch (Exception ex)
                {
                    return new ExceptionResult<T>(ex, new Oration("An exception occurred"));
                }
            }
        };
        if (QueueAction(() =>
            {
                if (task.BuildRunner().TryGetValue(out var action, out var buildError))
                    action();
                else
                    Console.WriteLine(buildError);
            }).OnError(out var queueError))
            return Task.FromResult(queueError.Cast<T>());

        return task.WaitResult();
    }

    public Task<Result<T>> BuildTask<T>(Func<Task<T>> func)
    {
        var task = new MaxiAsyncTask<T>
        {
            Action = async ct =>
            {
                try
                {
                    return Res.Value(await func());
                }
                catch (Exception ex)
                {
                    return new ExceptionResult<T>(ex, new Oration("An exception occurred"));
                }
            }
        };
        if (QueueAction(() => task.BuildRunner()).OnError(out var queueError))
            return Task.FromResult(queueError.Cast<T>());

        return task.WaitResult();
    }


    private sealed class PoolContext(SharedTaskPool pool) : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object? s)
        {
            pool.QueueAction(() => d(s));
        }


        public override SynchronizationContext CreateCopy()
        {
            return this;
        }
    }
}