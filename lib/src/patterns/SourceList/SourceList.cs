namespace MaxiNet;

public class SourceList<T> : ISource<T>, IAllocateSource<T>, ICompleteDeleteSource<T>, IEntitySource<T>,
    IEntitySourceQuery<T>, ISourceDeleteByQuery<T>, ISourceDeleteByIdentifier<T>, IEntitySourceEditor<T>
{
    public required string PrimaryKey { get; init; }
    public required Func<T, uint> IdentifierGetter { get; init; }

    public required Func<T, uint, Result<Nothing>> IdentifierSetter { get; init; }

    public ICollection<T> Content { get; init; } = new List<T>();


    public async Task<Result<Nothing>> Allocate(IAsyncEnumerable<Result<ICollection<T>>> content)
    {
        var result = Res.Ok;
        await foreach (var res in content)
        {
            if (!res.TryGetValue(out var list, out var error))
            {
                result = error.Cast<Nothing>();
                break;
            }

            foreach (var item in list)
            {
                var id = IdentifierGetter(item);
                var exists = Content.FirstOrDefault(x => IdentifierGetter(x) == id);
                if (exists != null)
                {Content.Remove(exists);
                    
                }
                Content.Add(item);

        }
        }


        return result;
    }

    public Task<Result<Nothing>> DeleteAll()
    {
        try
        {
            Content.Clear();
            return Task.FromResult( Res.Ok);
        }
        catch (ex)
        {
            return Task.FromResult(new ExceptionResult <Nothing>(ex, new Oration("Source is read-only and cannot be deleted ")));
        }

    }

   

    public uint ObtainIdentifier(T entity)
    {
        return IdentifierGetter(entity);
    }

    public Result<Nothing> ChangeIdentifier(T entity, uint identifier)
    {
        return IdentifierSetter(entity, identifier);
    }

    public Task<Result<Nothing>> Aggregate(IAsyncEnumerable<Result<ICollection<T>>> content, bool zeroKeyAutoAssign)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Nothing>> Modifier(IAsyncEnumerable<Result<ICollection<T>>> content, bool zeroKeyAutoAssign)
    {
        throw new NotImplementedException();
    }


    public IAsyncEnumerable<Result<ICollection<uint>>> QueryIdentifiers(List<ICondition>? conditions,
        uint? limit = null, OrderType order = OrderType.Disordered)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<Result<ICollection<Dictionary<uint, bool>>>> CheckAvailability(List<uint>? identifiers,
        List<ICondition>? conditions, uint? limit = null,
        OrderType order = OrderType.Disordered)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<Result<ICollection<T>>> Select(List<uint> identifiers)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<Result<ICollection<T>>> CheckAndSelect(List<uint> identifiers)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<uint>> Count(List<ICondition>? conditions)
    {
        if (conditions == null) return Res.Value((uint)Content.Count);

        uint result = 0;
        await foreach (var filterResult in Query(conditions))
            if (filterResult.TryGetValue(out var list, out var error))
                result += (uint)list.Count;
            else
                return error.Cast<uint>();

        return Res.Value(result);
    }

    public  IAsyncEnumerable<Result<ICollection<T>>> Query(List<ICondition>? conditions, uint? limit = null,
        OrderType order = OrderType.Disordered) => SourceListQuery.Query(this, conditions, limit, order);
    


    public Task<Result<Nothing>> DeleteByIdentifier(IAsyncEnumerable<Result<ICollection<uint>>> content)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Nothing>> DeleteByQuery(IAsyncEnumerable<Result<ICollection<T>>> content)
    {
        throw new NotImplementedException();
    }
}