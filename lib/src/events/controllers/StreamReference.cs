namespace MaxiNet.events.controller;

internal class StreamReference<T, TR> : Initializable, IStream<TR>
{
    public required IStream<T> MainSource { get; init; }
    public required Func<T,TR> Transform { get; init; }
    public  Predicate<T>?  Predicate { get; init; }
    
    public bool IsDisposed { get; private set; } =  false;
    
    private LinkedList<Action<TR>> _actions = new LinkedList<Action<TR>>();
    private LinkedList<Action> _closedActions = new LinkedList<Action>();
    
    public Result<Nothing> Listen(Action<TR> onItem, Action? onClosed)
    {
        if(IsDisposed) return Res.Error("Stream is disposed");
        if(Initialize().OnError(out var initError)) return initError;

        _actions.AddLast(onItem);
        if(onClosed != null) _closedActions.AddLast(onClosed);
        return Res.Ok;
    }

    protected override Result<Nothing> PerformInitialization()
    {
        if(IsDisposed || MainSource.IsDisposed) return Res.Error("Stream is disposed");
        if (MainSource.Listen(_onItem, _onCancel).OnError(out var error)) return error.Cast<Nothing>(); 
        
        return Res.Ok;

    }

    

    private void _onItem(T obj)
    {
        if(IsDisposed) return;
        if (Predicate != null && !Predicate(obj)) return;

        var item = Transform(obj);
        _actions.Lambda((act)=> act(item));
    }
    
    private void _onCancel()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (!IsDisposed)
        {
            IsDisposed = true;
            _actions.Clear();

            foreach (var act in _closedActions) Res.Volatile(new Oration("An error occurred while executing a closing function"), act.Invoke);
            
            _closedActions.Clear();
        }
    }

    
}