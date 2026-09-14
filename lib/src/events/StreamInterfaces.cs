namespace MaxiNet;

public interface IStream<T> : IMaxiDisposable
{
    public Result<Nothing> Listen(Action<T> onItem, Action? onClosed);
}

public interface IStreamController<T> : IMaxiDisposable
{
    public Result<IStream<T>> BuildStream();

    public Result<Nothing> AddItem(T item);
}

internal interface IStreamChildForController<T> : IMaxiDisposable
{
    public int Identifier { get; }
    public void DeclareNewItem(T item);
    public void DeclareAsClosed();
}

internal interface IStreamControllerForChild<T>
{
    public bool ChildConsultsActivity(IStreamChildForController<T> child);
    public void ChildDeclaredClosed(IStreamChildForController<T> child);
}