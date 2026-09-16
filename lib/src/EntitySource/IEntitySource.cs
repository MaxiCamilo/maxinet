namespace MaxiNet;

public enum OrderType
{
    Disordered,
    Ascending,
    Descending
}

public interface IQuerySource<T>
{
    public IAsyncEnumerable<Result<ICollection<T>>> Query(List<ICondition>? conditions, uint? limit = null,
        OrderType order = OrderType.Disordered);

    public Task<Result<uint>> Count(List<ICondition>? conditions);
}

public interface IAllocateSource<T>
{
    public Task<Result<Nothing>> Allocate(IAsyncEnumerable<Result<ICollection<T>>> content);
}

public interface ICompleteDeleteSource<T> 
{
    public Task<Result<Nothing>> DeleteAll();
}

public interface IEntitySource<in T>
{
    public string PrimaryKey { get; }

    public uint ObtainIdentifier(T entity);
    public Result<Nothing> ChangeIdentifier(T entity, uint identifier);
}

public interface IEntitySourceQuery<T>
{
    public Task<Result<uint>> ObtainMaxIdentifier(List<ICondition>? conditions);
    public Task<Result<uint>> ObtainMinIdentifier(List<ICondition>? conditions);
    
    public IAsyncEnumerable<Result<ICollection<uint>>> QueryIdentifiers(List<ICondition>? conditions,
        uint? limit = null, OrderType order = OrderType.Disordered);

    public IAsyncEnumerable<Result<Dictionary<uint, bool>>> CheckAvailability(List<uint>? identifiers,
        List<ICondition>? conditions, uint? limit = null, OrderType order = OrderType.Disordered);

    public IAsyncEnumerable<Result<ICollection<T>>> Select(List<uint> identifiers, uint? parts = null);

    public IAsyncEnumerable<Result<ICollection<T>>> CheckAndSelect(List<uint> identifiers, uint? parts = null);
}

public interface ISourceDeleteByQuery
{
    public Task<Result<Nothing>> DeleteByQuery(List<ICondition> conditions);
}

public interface ISourceDeleteByIdentifier
{
    public Task<Result<Nothing>> DeleteByIdentifier(IAsyncEnumerable<Result<ICollection<uint>>> content);
}

public interface IEntitySourceEditor<T> :  IEntitySourceQuery<T>
{
    public Task<Result<Nothing>> Aggregate(IAsyncEnumerable<Result<ICollection<T>>> content, bool zeroKeyAutoAssign);
    public Task<Result<Nothing>> Modifier(IAsyncEnumerable<Result<ICollection<T>>> content);
}