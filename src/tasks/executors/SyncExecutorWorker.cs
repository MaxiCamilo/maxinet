using System;
using System.Collections.Concurrent;

namespace MaxiNet;

public class SyncExecutorWorker : Disposable, ITaskExecutor
{
    private readonly BlockingCollection<Action> _queue = new(new ConcurrentQueue<Action>());

    public string Name { get; init; } = "Sync executor worker";

    public bool HasPendingTasks => !IsDisposed && _queue.Count > 0;

    public int PendingTaskCount => IsDisposed ? 0 : _queue.Count;

    public Task<Result<T>> BuildTask<T>(Func<Result<T>> work)
    {
        var tcs = new TaskCompletionSource<Result<T>>(TaskCreationOptions.RunContinuationsAsynchronously);

        _queue.Add(() =>
        {
            try
            {
                var result = work();
                tcs.TrySetResult(result);
            }
            catch (Exception ex)
            {
                tcs.TrySetResult(new ExceptionResult<T>(ex, new Oration("An exception occurred during task execution: ?", [ex.Message])));
            }
        });

        return tcs.Task;
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

