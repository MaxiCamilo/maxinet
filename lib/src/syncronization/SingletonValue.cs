namespace MaxiNet;

public class SingletonValue<T> : Disposable
{
    private readonly IStreamController<T> _changeController = StreamController<T>.ThreadSafe();

    private readonly EnqueueTask _enqueueTask = new();

    private bool _hasValue;
    private T? _value;

    public Result<IStream<T>> NotifyChange()
    {
        return _changeController.BuildStream();
    }

    public Task<Result<T>> GetValue()
    {
        return _enqueueTask.Add(() =>
            !_hasValue ? Res.ValError<T>(new Oration("Value has not been set")) : Res.Value<T>(_value!));
    }

    public Task<Result<Nothing>> SetValue(T value)
    {
        return _enqueueTask.Add(() =>
        {
            _value = value;
            _hasValue = true;
            _changeController.AddItem(value);
            return Res.Ok;
        });
    }

    public Task<Result<TR>> Ruminate<TR>(Func<T, TR> func)
    {
        return _enqueueTask.Add(() =>
            !_hasValue ? Res.ValError<TR>(new Oration("Value has not been set")) : Res.Value<TR>(func(_value!)));
    }

    public Task<Result<TR>> Ruminate<TR>(Func<T, Result<TR>> func)
    {
        return _enqueueTask.Add(() =>
            !_hasValue ? Res.ValError<TR>(new Oration("Value has not been set")) : func(_value!));
    }

    public Task<Result<Nothing>> Ruminate(Action<T> func)
    {
        return _enqueueTask.Add(() =>
            !_hasValue ? Res.ValError<Nothing>(new Oration("Value has not been set")) : Res.Ok);
    }

    protected override void PerformDispose()
    {
        base.PerformDispose();

        _changeController.Dispose();
        _enqueueTask.Dispose();
    }
}