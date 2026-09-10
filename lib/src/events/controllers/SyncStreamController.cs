namespace MaxiNet.events.controller;

internal class SyncStreamController<T> : Disposable, IStreamController<T>, IStreamControllerForChild<T>
{
    private readonly List<IStreamChildForController<T>> _children = [];

    int _lastID = 1;


    public Result<IStream<T>> BuildStream()
    {
        if (this.ErrorIfDispose() is IFailure failure) return failure.Cast<IStream<T>>();

        var id = _lastID;
        _lastID += 1;

        var child = new SyncStream<T>() { controller = this, Identifier = id };
        _children.Add(child);
        return Res.Value<IStream<T>>(child);
    }



    public Result<Nothing> AddItem(T item)
    {
        if (this.ErrorIfDispose() is ResultFailure<Nothing> failure) return failure;

        foreach (var child in _children)
        {

            child.DeclareNewItem(item);

        }

        return Res.Ok;
    }



    public bool ChildConsultsActivity(IStreamChildForController<T> child)
    {
        return !IsDisposed;
    }

    public void ChildDeclaredClosed(IStreamChildForController<T> child)
    {
        if (IsDisposed) return;

        var realInstance = _children.FirstOrDefault(c => c.Identifier == child.Identifier);
        if (realInstance != null)
        {
            _children.Remove(realInstance);

        }
    }



    protected override void PerformDispose()
    {
        var clon = _children.ToArray();
        _children.Clear();
        foreach (var child in clon)
        {
            child.DeclareAsClosed();
        }


    }


}

internal class SyncStream<T> : Disposable, IStream<T>, IStreamChildForController<T>
{
    private bool _declaredClosed = false;
    private LinkedList<Action<T>> _listeners = new LinkedList<Action<T>>();
    private LinkedList<Action> _closedListeners = new LinkedList<Action>();

    public required int Identifier
    {
        get; init;
    }

    public required IStreamControllerForChild<T> controller
    {
        get; init;
    }


    public Result<Nothing> Listen(Action<T> onItem, Action? onClosed)
    {
        if (this.ErrorIfDispose() is IFailure failure) return failure.Cast<Nothing>();

        _listeners.AddLast(onItem);
        if (onClosed != null)
        {
            _closedListeners.AddLast(onClosed);
        }
        return Res.Ok;
    }



    public void DeclareNewItem(T item)
    {

        foreach (var listener in _listeners)
        {
            try
            {
                listener(item);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception thrown while processing item in stream: {ex}. This is wrong!");
            }
        }

    }

    public void DeclareAsClosed()
    {
        if (_declaredClosed)
        {
            return;
        }

        _declaredClosed = true;
        Dispose();

    }

    protected override void PerformDispose()
    {
        if (!_declaredClosed)
        {
            controller.ChildDeclaredClosed(this);
        }

        _listeners.Clear();


        try
        {
            foreach (var closedListener in _closedListeners)
            {
                try
                {
                    closedListener();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception thrown while processing stream closure: {ex}. This is wrong!");
                }
            }
        }


        catch (Exception ex)
        {
            Console.WriteLine($"Exception thrown while processing stream closure: {ex}. This is wrong!");
        }

        _closedListeners.Clear();

    }
}