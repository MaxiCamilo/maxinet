namespace MaxiNet;

public class Lifecycle : Disposable
{
    private readonly LinkedList<Disposable> _children = new();

    private readonly LinkedList<TemporalLifecycle> _lifeChildren = new();
    private readonly LinkedList<Action> _onDisposeActions = new();

    public Result<Disposable> AddChild(Disposable disposable)
    {
        if (this.ErrorIfDispose() is IFailure failure) return failure.Cast<Disposable>();
        var temp = new TemporalChild(disposable);
        _children.AddFirst(temp);
        return Res.Value<Disposable>(temp);
    }

    public Result<Nothing> OnDispose(Action action)
    {
        if (this.ErrorIfDispose() is ResultFailure<Nothing> failure) return failure;
        _onDisposeActions.AddFirst(action);
        return Res.Ok;
    }

    public Result<T> Build<T>(Func<T> func) where T : Disposable
    {
        if (this.ErrorIfDispose() is IFailure failure) return failure.Cast<T>();
        var item = func();
        var temp = new TemporalChild(item);
        _children.AddFirst(temp);
        return Res.Value(item);
    }

    public Result<Nothing> AttachLifecycle(Lifecycle lifecycle)
    {
        if (this.ErrorIfDispose() is IFailure failure) return failure.Cast<Nothing>();
        if (lifecycle.IsDisposed) return Res.Error("The lifecycle to attach is already disposed");

        _ = new TemporalLifecycle(lifecycle, _lifeChildren);


        return Res.Ok;
    }

    protected override void PerformDispose()
    {
        foreach (var child in _children) child.Dispose();
        _children.Clear();

        foreach (var action in _onDisposeActions) action();
        _onDisposeActions.Clear();

        foreach (var lifecycle in _lifeChildren) lifecycle?.Dispose();
        _lifeChildren.Clear();
    }
}

internal class TemporalChild(Disposable item) : Disposable
{
    private Disposable? _item = !item.IsDisposed ? item : null;

    protected override void PerformDispose()
    {
        _item?.Dispose();
        _item = null;
    }
}

internal class TemporalLifecycle : Disposable
{
    private readonly Lifecycle _item;
    private readonly LinkedList<TemporalLifecycle> _lifecyclesList;
    private readonly LinkedListNode<TemporalLifecycle> _node;

    public TemporalLifecycle(Lifecycle item, LinkedList<TemporalLifecycle> lifecyclesList)
    {
        _item = item;
        _lifecyclesList = lifecyclesList;
        _node = lifecyclesList.AddFirst(this);


        _item.OnDispose(() =>
        {
            if (IsDisposed) return;
            _lifecyclesList.Remove(_node);
        });
    }

    protected override void PerformDispose()
    {
        _lifecyclesList.Remove(_node);
        _item.Dispose();
    }
}