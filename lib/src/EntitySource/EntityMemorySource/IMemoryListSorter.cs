using System.Diagnostics.CodeAnalysis;

namespace MaxiNet;

internal interface IMemoryListSorter<T>
{
    public int Count { get; }
    public Result<Nothing> Set(uint id, T item);
    public Result<Nothing> SetAll(IDictionary<uint,T> values);
    
    public Result<Nothing> RemoveAll();
    public Result<Nothing> Remove(uint id);
    public Result<Nothing> RemoveAll(IEnumerable<uint> ids);
    
    public Result<bool> Exists(uint id);
    public IEnumerable<KeyValuePair<uint,bool>> Exists(IEnumerable<uint> ids);
    public bool Obtain(uint id,[NotNullWhen(true)] out T? item);
    public Result<ICollection<T>> ObtainAll(IEnumerable<uint> ids);

    IEnumerable< KeyValuePair<uint,T>> AscendingEnumerable(uint from = 0);
    IEnumerable< KeyValuePair<uint,T>> DescendingEnumerable(uint from = 0);

    public uint ObtainMax();
    
    public uint ObtainMin();

    public T? ObtainItem(uint id);

    public IEnumerable<KeyValuePair<uint, T?>> ObtainMultipleItems(List<uint> ids);
}