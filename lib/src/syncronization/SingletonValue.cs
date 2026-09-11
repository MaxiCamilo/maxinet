namespace MaxiNet;

public class SingletonValue<T> : Disposable
{
    private T? _value;

    private bool _hasValue = false;

    private EnqueueTask _enqueueTask = new();

    private IStreamController<T> _changeController = StreamController<T>.ThreadSafe();

    public Result<IStream<T>> NotifyChange()
    {
        return _changeController.BuildStream();
    }

    public Task<Result<T>> GetValue()
    {
        return _enqueueTask.Add(() =>
        {
            if (!_hasValue)
            {
                return Res.ValError<T>(new Oration("Value has not been set"));
            }


            return Res.Value<T>(_value!);
        });
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

    public Task<Result<R>> Ruminate<R>(Func<T, R> func)
    {
        return _enqueueTask.Add(() =>
        {
            if (!_hasValue)
            {
                return Res.ValError<R>(new Oration("Value has not been set"));
            }

            return Res.Value<R>(func(_value!));
        });
    }

    public Task<Result<R>> Ruminate<R>(Func<T, Result<R>> func)
    {
        return _enqueueTask.Add(() =>
        {
            if (!_hasValue)
            {
                return Res.ValError<R>(new Oration("Value has not been set"));
            }

            return func(_value!);
        });
    }

    public Task<Result<Nothing>> Ruminate(Action<T> func)
    {
        return _enqueueTask.Add(() =>
        {
            if (!_hasValue)
            {
                return Res.ValError<Nothing>(new Oration("Value has not been set"));
            }

            return Res.Ok;
        });
    }

    protected override void PerformDispose()
    {
        base.PerformDispose();

        _changeController.Dispose();
        _enqueueTask.Dispose();
    }




}

