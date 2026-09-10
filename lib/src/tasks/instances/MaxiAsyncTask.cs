using System;
using System.Threading;
using System.Threading.Tasks;
namespace MaxiNet;

internal class MaxiAsyncTask<T> : Initializable
{
    public required Func<CancellationToken, Task<Result<T>>> Action { get; init; }

    public CancellationToken CancellationToken { get; init; } = CancellationToken.None;

    private TaskCompletionSource<Result<T>>? _completion;
    private ExecutionContext? _executionContext;



    protected override Result<Nothing> PerformInitialization()
    {
        _executionContext = ExecutionContext.Capture();

        return Res.Ok;
    }



    public Task<Result<T>> Run(Func<Action, dynamic> postAction)
    {
        var initResult = Initialize();
        if (initResult is IFailure failure) return Task.FromResult(failure.Cast<T>());

        if (_completion is not null) throw new ArgumentException("The task has already been started");


        _completion = new TaskCompletionSource<Result<T>>(TaskCreationOptions.RunContinuationsAsynchronously);


        Action envuelto = _executionContext is null
            ? StarOrResumeTask
            : () => ExecutionContext.Run(_executionContext, static s => ((MaxiAsyncTask<T>)s!).StarOrResumeTask(), this);

        postAction(envuelto);

        return _completion!.Task;
    }



    private void StarOrResumeTask()
    {
        if (CancellationToken.IsCancellationRequested) { _completion?.TrySetCanceled(CancellationToken); return; }

        Task<Result<T>> running;
        try { running = Action(CancellationToken); }
        catch (OperationCanceledException oce) when (CancellationToken.IsCancellationRequested)
        {
            _completion?.TrySetCanceled(oce.CancellationToken);
            return;
        }
        catch (Exception ex)
        {
            _completion?.TrySetResult(new ExceptionResult<T>(ex, new Oration("An exception was thrown while executing the task")));
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



