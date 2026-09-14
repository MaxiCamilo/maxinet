namespace MaxiNet;

public interface ITocInstance
{
    public Task<Result<Nothing>> Add(Action action);
    public Task<Result<T>> Add<T>(Func<T> function);

    public Task<Result<T>> Add<T>(Func<Task<T>> function);

    public Task<Result<T>> Add<T>(Func<Task<Result<T>>> function);
}

internal class TocInstance : Initializable, ITocInstance
{
    private MaxiThreadInstance[] _threads = Array.Empty<MaxiThreadInstance>();
    public int ThreadCount { get; init; } = Environment.ProcessorCount;
    public SharedTaskPool TaskPool { get; init; } = new();

    public Task<Result<Nothing>> Add(Action action)
    {
        return TaskPool.BuildTask(() =>
        {
            action();
            return Res.Ok;
        });
    }

    public Task<Result<T>> Add<T>(Func<T> function)
    {
        return TaskPool.BuildTask(() => Res.Value(function()));
    }

    public Task<Result<T>> Add<T>(Func<Task<T>> function)
    {
        return TaskPool.BuildTask(function);
    }

    public Task<Result<T>> Add<T>(Func<Task<Result<T>>> function)
    {
        return TaskPool.BuildTask(function);
    }

    protected override Result<Nothing> PerformInitialization()
    {
        _threads = new MaxiThreadInstance[ThreadCount];
        for (var i = 0; i < ThreadCount; i++)
        {
            var thread = new MaxiThreadInstance
            {
                Name = $"Thread-{i}",
                Identifier = i,
                TaskPool = TaskPool
            };

            if (thread.StartThread().OnError(out var error))
            {
                for (var z = 0; z < i; z++) _threads[z].Dispose();
                return error.Cast<Nothing>();
            }

            _threads[i] = thread;
        }


        return Res.Ok;
    }
}