namespace MaxiNet.EntityMemorySource;

internal class EntitySourceQuery<T> : IEntitySourceQuery<T>
{
    public required Func<T, uint> IdentifierGetter{ get; init; }
    public required IMemoryListSorter<T>  Sorter { get; init; }
    
    public Task<Result<uint>> ObtainMaxIdentifier(List<ICondition>? conditions)
    {
        throw new NotImplementedException();
    }

    public Task<Result<uint>> ObtainMinIdentifier(List<ICondition>? conditions)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<Result<ICollection<uint>>> QueryIdentifiers(List<ICondition>? conditions, uint? limit = null, OrderType order = OrderType.Disordered)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<Result<Dictionary<uint, bool>>> CheckAvailability(List<uint>? identifiers, List<ICondition>? conditions, uint? limit = null,
        OrderType order = OrderType.Disordered)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<Result<ICollection<T>>> Select(List<uint> identifiers, uint? parts = null)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<Result<ICollection<T>>> QueryRange(uint? maximum, List<ICondition>? conditions, uint from = 0, uint? limit = null,
        OrderType order = OrderType.Disordered)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<Result<ICollection<T>>> CheckAndSelect(List<uint> identifiers, uint? parts = null)
    {
        throw new NotImplementedException();
    }
}