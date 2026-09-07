using System;
using System.Collections.Concurrent;

namespace MaxiNet;

internal sealed class AsynchronousTaskExecutorContext(AsynchronousTaskExecutor worker) : SynchronizationContext
{
    public override void Post(SendOrPostCallback d, object? s) => worker._queue.Add(() => d(s));
    public override SynchronizationContext CreateCopy() => this;
}

public sealed class AsynchronousTaskExecutor : Disposable, ITaskExecutor
{
    private readonly AsynchronousTaskExecutorContext _context;
    internal readonly BlockingCollection<Action> _queue = new(new ConcurrentQueue<Action>());

    public string Name { get; init; } = "Async Worker";



    public bool HasPendingTasks => !IsDisposed && _queue.Count > 0;

    public int PendingTaskCount => IsDisposed ? 0 : _queue.Count;

    public AsynchronousTaskExecutor()
    {
        _context = new(this);
    }

    public Task<Result<T>> BuildTask<T>(Func<Task<Result<T>>> work)
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(GetType().Name, "Cannot build a task on a disposed executor");
        }



        var tcs = new TaskCompletionSource<Result<T>>(TaskCreationOptions.RunContinuationsAsynchronously);

        _queue.Add(() =>
        {
            try
            {
                work().ContinueWith(
                    t => Complete(tcs, t),
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                tcs.TrySetResult(new ExceptionResult<T>(ex, new Oration("An exception occurred during task execution: ?", [ex.Message])));

            }
        });

        return tcs.Task;

    }

    private static void Complete<T>(TaskCompletionSource<Result<T>> tcs, Task<Result<T>> t)
    {
        switch (t.Status)
        {
            case TaskStatus.RanToCompletion: tcs.TrySetResult(t.Result); break;
            case TaskStatus.Canceled: tcs.TrySetCanceled(); break;
            default: tcs.TrySetException(t.Exception!.InnerExceptions); break;
        }
    }

    public bool ExecuteTurn()
    {
        if (IsDisposed)
        { return false; }

        if (!_queue.TryTake(out var action))
        {
            return false;
        }

        action();

        return HasPendingTasks;
    }



    protected override void PerformDispose()
    {
        _queue.CompleteAdding();
    }


}

