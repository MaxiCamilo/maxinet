using System.Collections.Concurrent;

namespace MaxiNet;

public class EnqueueTask : Disposable
{
    private readonly Lock _lock = new();
    private readonly ConcurrentQueue<Func<Task>> _tasks = new();
    private bool _isActive;

    private async Task<Result<Nothing>> StartLoop()
    {
        lock (_lock)
        {
            _isActive = true;
        }

        while (true)
        {
            if (!_tasks.TryDequeue(out var task)) break;

            await task();

            if (_tasks.IsEmpty) break;
        }

        lock (_lock)
        {
            _isActive = false;
        }

        return Res.Ok;
    }

    private void EnsureLoopRunning()
    {
        if (_isActive) return;

        lock (_lock)
        {
            if (!_isActive) _ = StartLoop();
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
        _tasks.Enqueue(() =>
        {
            try
            {
                try
                {
                    var result = function();
                    waiter.SetResult(result);
                }
                catch (Exception ex)
                {
                    waiter.SetResult(new ExceptionResult<T>(ex,
                        new Oration("An error occurred while executing the task")));
                }

                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        });

        EnsureLoopRunning();
        return await waiter.Task;
    }

    public Task<Result<T>> Add<T>(Func<T> function)
    {
        return Add(() => Res.Value(function()));
    }

    public Task<Result<Nothing>> Add(Action function)
    {
        return Add(() =>
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