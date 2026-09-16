namespace MaxiNet.EntityMemorySource;

internal class QuerySource<T>: IQuerySource<T>
{
    public required IMemoryListSorter<T>  Sorter { get; init; }

    public required Func<T, uint> IdentifierGetter{ get; init; }
    public required Func<T, uint, Result<Nothing>> IdentifierSetter{ get; init; }
    
    public async IAsyncEnumerable<Result<ICollection<T>>> Query(List<ICondition>? conditions, uint? limit = null, OrderType order = OrderType.Disordered)
    {
        var iterator = order == OrderType.Descending ? Sorter.DescendingEnumerable() : Sorter.AscendingEnumerable();
        ICollection<T> result = (limit is 0 or null) ? new LinkedList<T>() : new List<T>((int)limit);

        EntityMemorySourceConditioner<T>? conditioner = null;
        if (conditions is { Count: > 0 })
        {
            conditioner = new EntityMemorySourceConditioner<T>()
            {
                IdentifierGetter = IdentifierGetter,
                Conditions = conditions,
            };
        }
        

        foreach (var item in iterator)
        {
            if ( conditioner != null && !conditioner.Execute(item.Value))
            {
                continue;
            }
            
            result.Add(item.Value);
            if (limit == null || !(result.Count >= limit)) continue;
            yield return Res.Value(result);
            result.Clear();
        }

        if (result.Count > 0)
        {
            yield return Res.Value(result);
        }

    }

    public async Task<Result<uint>> Count(List<ICondition>? conditions)
    {
        if (conditions is { Count: > 0 })
        {
            uint  count = 0;
            await foreach (var listRes in Query(conditions))
            {
                if (listRes.TryGetValue(out var list, out var error))
                {
                    count += (uint)list.Count;
                }
                else
                {
                    return error.Cast<uint>();
                }
            }

            return Res.Value(count);
        }
        else
        {
            return Res.Value((uint)Sorter.Count);
        }
    }
}