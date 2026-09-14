namespace MaxiNet;

public enum OrderType
{
    Disordered,
    Ascending,
    Descending
}

public interface ISource<T>
{
    public IAsyncEnumerable<Result<ICollection<T>>> Query(List<ICondition>? conditions, uint? limit = null,
        OrderType order = OrderType.Disordered);

    public Task<Result<uint>> Count(List<ICondition>? conditions);
}

public interface IAllocateSource<T> : ISource<T>
{
    public Task<Result<Nothing>> Allocate(IAsyncEnumerable<Result<ICollection<T>>> content);
}

public interface ICompleteDeleteSource<T> : ISource<T>
{
    public Task<Result<Nothing>> DeleteAll();
}

public interface IEntitySource<T> : ISource<T>
{
    public string PrimaryKey { get; }

    public uint ObtainIdentifier(T entity);
    public Result<Nothing> ChangeIdentifier(T entity, uint identifier);
}

public interface IEntitySourceQuery<T> : ISource<T>
{
    public IAsyncEnumerable<Result<ICollection<uint>>> QueryIdentifiers(List<ICondition>? conditions,
        uint? limit = null, OrderType order = OrderType.Disordered);

    public IAsyncEnumerable<Result<ICollection<Dictionary<uint, bool>>>> CheckAvailability(List<uint>? identifiers,
        List<ICondition>? conditions, uint? limit = null, OrderType order = OrderType.Disordered);

    public IAsyncEnumerable<Result<ICollection<T>>> Select(List<uint> identifiers);

    public IAsyncEnumerable<Result<ICollection<T>>> CheckAndSelect(List<uint> identifiers);
}

public interface ISourceDeleteByQuery<T>
{
    public Task<Result<Nothing>> DeleteByQuery(IAsyncEnumerable<Result<ICollection<T>>> content);
}

public interface ISourceDeleteByIdentifier<T> : ISource<T>
{
    public Task<Result<Nothing>> DeleteByIdentifier(IAsyncEnumerable<Result<ICollection<uint>>> content);
}

public interface IEntitySourceEditor<T> : ISource<T>, IEntitySourceQuery<T>
{
    public Task<Result<Nothing>> Aggregate(IAsyncEnumerable<Result<ICollection<T>>> content, bool zeroKeyAutoAssign);
    public Task<Result<Nothing>> Modifier(IAsyncEnumerable<Result<ICollection<T>>> content, bool zeroKeyAutoAssign);
}