using System;
using System.Collections.Concurrent;

namespace MaxiNet.events.controller;

public class ThreadSafeStreamController<T> : Disposable, IStreamController<T>, IStreamControllerForChild<T>
{
    int _lastID = 1;
    private readonly object _lock = new();

    private readonly List<IStreamChildForController<T>> _children = [];

    private IStreamChildForController<T>[] Snapshot()
    {
        lock (_lock)
        {
            return _children.ToArray();
        }
    }

    public Result<Nothing> AddItem(T item)
    {
        if (this.ErrorIfDispose() is IFailure failure) return failure.Cast<Nothing>();

        var copyChildren = Snapshot();
        for (int i = 0; i < copyChildren.Length; i++)
        {
            if (IsDisposed)
            {
                return Res.Error("The stream controller has been disposed during operation");
            }

            var child = copyChildren[i];
            if (child.IsDisposed) continue;
            child.DeclareNewItem(item);

        }

        return Res.Ok;
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
            if (realInstance != null)
            {
                _children.Remove(realInstance);

            }
        }
    }

    override protected void PerformDispose()
    {
        lock (_lock)
        {
            var clon = _children.ToArray();
            _children.Clear();

            foreach (var child in clon)
            {
                child.Dispose();
            }
        }
    }

    public Result<IStream<T>> BuildStream()
    {

        lock (_lock)
        {
            if (this.ErrorIfDispose() is IFailure failure) return failure.Cast<IStream<T>>();
            var id = _lastID;
            _lastID += 1;

            var child = new ThreadSafeStream<T>() { Controller = this, Identifier = id };

            _children.Add(child);
            return Res.Value<IStream<T>>(child);
        }

    }
}

internal class ThreadSafeStream<T> : Disposable, IStream<T>, IStreamChildForController<T>
{
    bool _declaredClosed = false;
    bool _callOnClosed = false;

    private LinkedList<Action<T>> _listeners = new LinkedList<Action<T>>();
    private LinkedList<Action> _closedListeners = new LinkedList<Action>();

    private readonly object _lock = new();

    public required int Identifier
    {
        get; init;
    }

    public required IStreamControllerForChild<T> Controller
    {
        get; init;
    }




    public void DeclareAsClosed()
    {
        if (_declaredClosed)
        {
            return;
        }

        _declaredClosed = true;
        Controller.ChildDeclaredClosed(this);
        Dispose();
    }

    public Result<Nothing> Listen(Action<T> onItem, Action? onClosed)
    {
        if (this.ErrorIfDispose() is IFailure failure) return failure.Cast<Nothing>();


        lock (_lock)
        {
            if (this.ErrorIfDispose() is IFailure futureFailure) return futureFailure.Cast<Nothing>();

            _listeners.AddLast(onItem);
            if (onClosed != null)
            {
                _closedListeners.AddLast(onClosed);
            }
        }
        return Res.Ok;


    }

    public void DeclareNewItem(T item)
    {
        if (IsDisposed || _declaredClosed)
        {
            return;
        }


        lock (_lock)
        {
            if (IsDisposed || _declaredClosed)
            {
                return;
            }

            foreach (var listener in _listeners)
            {
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
    }

    protected override void PerformDispose()
    {
        lock (_lock)
        {
            if (_callOnClosed)
            {
                return;
            }

            try
            {
                _callOnClosed = true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception thrown while processing stream closure: {ex}. This is wrong!");
            }

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

            _closedListeners.Clear();
            _listeners.Clear();
        }


    }
}

