using System;
using System.Collections.Concurrent;

namespace MaxiNet;

public class EnqueueTask : Disposable
{
    private ConcurrentQueue<Func<Task>> _tasks = new ConcurrentQueue<Func<Task>>();
    private readonly object _lock = new object();
    private bool _isActive = false;

    private async Task<Result<Nothing>> StartLoop()
    {
        lock (_lock)
        {
            _isActive = true;
        }
        while (true)
        {
            if (!_tasks.TryDequeue(out var task))
            {
                break;
            }

            await task();

            if (_tasks.IsEmpty)
            {

                break;
            }
        }

        lock (_lock)
        {
            _isActive = false;
        }
        return Res.Ok;
    }

    private void EnsureLoopRunning()
    {
        if (_isActive)
        { return; }

        lock (_lock)
        {
            if (!_isActive)
            {
                _ = StartLoop();
            }
        }
    }


    public async Task<Result<T>> Add<T>(Func<Task<Result<T>>> function)
    {
        var waiter = new TaskCompletionSource<Result<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
        _tasks.Enqueue(async () =>
        {
            try
            {
                var result = await function();
                waiter.SetResult(result);
            }
            catch (Exception ex)
            {
                waiter.SetResult(new ExceptionResult<T>(ex, new Oration("An error occurred while executing the task")));
            }
        });

        EnsureLoopRunning();

        return await waiter.Task;
    }

    public async Task<Result<T>> Add<T>(Func<Result<T>> function)
    {
        var waiter = new TaskCompletionSource<Result<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
        _tasks.Enqueue(async () =>
        {
            try
            {
                var result = function();
                waiter.SetResult(result);
            }
            catch (Exception ex)
            {
                waiter.SetResult(new ExceptionResult<T>(ex, new Oration("An error occurred while executing the task")));
            }
        });

        EnsureLoopRunning();
        return await waiter.Task;
    }

    public Task<Result<T>> Add<T>(Func<T> function)
    {
        return Add<T>(() =>
        {
            return Res.Value(function());
        });
    }

    public Task<Result<Nothing>> Add(Action function)
    {
        return Add<Nothing>(() =>
        {
            function();
            return Res.Ok;
        });
    }
    protected override void PerformDispose()
    {
        base.PerformDispose();
        _tasks.Clear();
    }
}

