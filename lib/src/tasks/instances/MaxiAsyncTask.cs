namespace MaxiNet;

internal interface IMaxiAsyncTask
{
    public Result<Action> BuildRunner();
}

internal class MaxiAsyncTask<T> : Initializable, IMaxiAsyncTask
{
    private TaskCompletionSource<Result<T>>? _completion;
    private ExecutionContext? _executionContext;
    public required Func<CancellationToken, Task<Result<T>>> Action { get; init; }

    public CancellationToken CancellationToken { get; init; } = CancellationToken.None;


    public Result<Action> BuildRunner()
    {
        if (Initialize().OnError(out var initError)) return initError.Cast<Action>();

        _completion ??= new TaskCompletionSource<Result<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
        _executionContext ??= ExecutionContext.Capture();

        var envuelto =
            () => ExecutionContext.Run(_executionContext!, static s => ((MaxiAsyncTask<T>)s!).StarOrResumeTask(), this);


        return Res.Value(envuelto);
    }


    protected override Result<Nothing> PerformInitialization()
    {
        return Res.Ok;
    }


    public Task<Result<T>> WaitResult()
    {
        var initResult = Initialize();
        if (initResult is IFailure failure) return Task.FromResult(failure.Cast<T>());

        _completion ??= new TaskCompletionSource<Result<T>>(TaskCreationOptions.RunContinuationsAsynchronously);

        return _completion!.Task;
    }


    private void StarOrResumeTask()
    {
        if (CancellationToken.IsCancellationRequested)
        {
            _completion?.TrySetCanceled(CancellationToken);
            return;
        }

        Task<Result<T>> running;
        try
        {
            running = Action(CancellationToken);
        }
        catch (OperationCanceledException oce) when (CancellationToken.IsCancellationRequested)
        {
            _completion?.TrySetCanceled(oce.CancellationToken);
            return;
        }
        catch (Exception ex)
        {
            _completion?.TrySetResult(new ExceptionResult<T>(ex,
                new Oration("An exception was thrown while executing the task")));
            return;
        }

        running.ContinueWith(static (t, s) =>
            {
                var task = (MaxiAsyncTask<T>)s!;

                var box = task._completion;
                if (box is null) return;

                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion: box.TrySetResult(t.Result); break;
                    case TaskStatus.Canceled: box.TrySetCanceled(); break;
                    default: box.TrySetException(t.Exception!.InnerExceptions); break;
                }
            }, this, CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }
}