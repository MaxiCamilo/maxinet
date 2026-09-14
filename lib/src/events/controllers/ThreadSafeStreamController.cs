namespace MaxiNet.events.controller;

public class ThreadSafeStreamController<T> : Disposable, IStreamController<T>, IStreamControllerForChild<T>
{
    private readonly List<IStreamChildForController<T>> _children = [];
    private readonly Lock _lock = new();
    private int _lastId = 1;

    public Result<Nothing> AddItem(T item)
    {
        if (this.ErrorIfDispose() is IFailure failure) return failure.Cast<Nothing>();

        var copyChildren = Snapshot();
        for (var i = 0; i < copyChildren.Length; i++)
        {
            if (IsDisposed) return Res.Error("The stream controller has been disposed during operation");

            var child = copyChildren[i];
            if (child.IsDisposed) continue;
            child.DeclareNewItem(item);
        }

        return Res.Ok;
    }

    public Result<IStream<T>> BuildStream()
    {
        lock (_lock)
        {
            if (this.ErrorIfDispose() is IFailure failure) return failure.Cast<IStream<T>>();
            var id = _lastId;
            _lastId += 1;

            var child = new ThreadSafeStream<T> { Controller = this, Identifier = id };

            _children.Add(child);
            return Res.Value<IStream<T>>(child);
        }
    }


    bool IStreamControllerForChild<T>.ChildConsultsActivity(IStreamChildForController<T> child)
    {
        return !IsDisposed;
    }

    void IStreamControllerForChild<T>.ChildDeclaredClosed(IStreamChildForController<T> child)
    {
        if (IsDisposed) return;
        lock (_lock)
        {
            if (IsDisposed) return;

            var realInstance = _children.FirstOrDefault(c => c.Identifier == child.Identifier);
            if (realInstance != null) _children.Remove(realInstance);
        }
    }

    private IStreamChildForController<T>[] Snapshot()
    {
        lock (_lock)
        {
            return _children.ToArray();
        }
    }

    protected override void PerformDispose()
    {
        lock (_lock)
        {
            var clon = _children.ToArray();
            _children.Clear();

            foreach (var child in clon) child.Dispose();
        }
    }
}

internal class ThreadSafeStream<T> : Disposable, IStream<T>, IStreamChildForController<T>
{
    private readonly LinkedList<Action> _closedListeners = new();

    private readonly LinkedList<Action<T>> _listeners = new();

    private readonly Lock _lock = new();
    private bool _callOnClosed;
    private bool _declaredClosed;

    public required IStreamControllerForChild<T> Controller { get; init; }

    public Result<Nothing> Listen(Action<T> onItem, Action? onClosed)
    {
        if (this.ErrorIfDispose() is IFailure failure) return failure.Cast<Nothing>();


        lock (_lock)
        {
            if (this.ErrorIfDispose() is IFailure futureFailure) return futureFailure.Cast<Nothing>();

            _listeners.AddLast(onItem);
            if (onClosed != null) _closedListeners.AddLast(onClosed);
        }

        return Res.Ok;
    }

    public required int Identifier { get; init; }


    public void DeclareAsClosed()
    {
        if (_declaredClosed) return;

        _declaredClosed = true;
        Controller.ChildDeclaredClosed(this);
        Dispose();
    }

    public void DeclareNewItem(T item)
    {
        if (IsDisposed || _declaredClosed) return;


        lock (_lock)
        {
            if (IsDisposed || _declaredClosed) return;

            foreach (var listener in _listeners)
                try
                {
                    listener(item);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception thrown while processing stream item: {ex}. This is wrong!");
                }
        }
    }

    protected override void PerformDispose()
    {
        lock (_lock)
        {
            if (_callOnClosed) return;

            try
            {
                _callOnClosed = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception thrown while processing stream closure: {ex}. This is wrong!");
            }

            foreach (var closedListener in _closedListeners)
                try
                {
                    closedListener();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception thrown while processing stream closure: {ex}. This is wrong!");
                }

            _closedListeners.Clear();
            _listeners.Clear();
        }
    }
}