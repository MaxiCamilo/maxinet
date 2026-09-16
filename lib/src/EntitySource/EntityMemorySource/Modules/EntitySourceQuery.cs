namespace MaxiNet.EntityMemorySource;

internal class EntitySourceQuery<T> : IEntitySourceQuery<T>
{
    public required Func<T, uint> IdentifierGetter{ get; init; }
    public required IMemoryListSorter<T>  Sorter { get; init; }
    public required IQuerySource<T> Query { get; init; }
    
    public async Task<Result<uint>> ObtainMaxIdentifier(List<ICondition>? conditions)
    {
        if (conditions is { Count: > 1 })
        {
            uint values = 0;
            await foreach (var res in Query.Query(conditions))
            {
                if (res.TryGetValue(out var list, out var error))
                {
                    foreach (var item in list)
                    {
                        var id = IdentifierGetter(item);
                        if (id > values) values = id;
                    }
                }
                else
                {
                    return error.Cast<uint>();
                }
            }

            return Res.Value(values);
        }
        else
        {
            return Res.Value(Sorter.ObtainMax());
        }
    }

    public async Task<Result<uint>> ObtainMinIdentifier(List<ICondition>? conditions)
    {
        if (conditions is { Count: > 1 })
        {
            uint values = uint.MaxValue;
            await foreach (var res in Query.Query(conditions))
            {
                if (res.TryGetValue(out var list, out var error))
                {
                    foreach (var item in list)
                    {
                        var id = IdentifierGetter(item);
                        if (id < values) values = id;
                    }
                }
                else
                {
                    return error.Cast<uint>();
                }
            }

            return Res.Value(values == uint.MaxValue ? 0 :  values);
        }
        else
        {
            return Res.Value(Sorter.ObtainMin());
        }
    }

    public async IAsyncEnumerable<Result<ICollection<uint>>> QueryIdentifiers(List<ICondition>? conditions, uint? limit = null, OrderType order = OrderType.Disordered)
    {
        await foreach (var res in Query.Query(conditions, limit, order))
        {
            if (!res.TryGetValue(out var list, out var error))
            {
                yield return error.Cast<ICollection<uint>>();
                yield break;
            }

            yield return Res.Value<ICollection<uint>>(list.Select(IdentifierGetter).ToList());
        }
    }

    public async IAsyncEnumerable<Result<Dictionary<uint, bool>>> CheckAvailability(List<uint>? identifiers, List<ICondition>? conditions, uint? limit = null,
        OrderType order = OrderType.Disordered)
    {
        if (identifiers == null)
        {
            
            await foreach (var res in Query.Query(limit: limit, conditions:conditions,  order:order))
            {
                if (res.TryGetValue(out var list, out var error))
                {
                    var dir = new Dictionary<uint, bool>(list.Count);
                    list.Lambda(x => dir.Add(IdentifierGetter(x), true));
                    yield return Res.Value(dir);
                }
                else
                {
                    yield return error.Cast<Dictionary<uint, bool>>();
                    yield break;
                }
            } 
            yield break;
        }
        
        var copy = limit == null ? new List<List<uint>>([identifiers]) : identifiers.SplitIntoFixedParts((int)limit);
        foreach (var part in copy)
        {
            var dic = Sorter.ObtainMultipleItems(part).ToDictionary(item => item.Key, item => item.Value != null);

            yield return Res.Value(dic);
        }
    }
    
    

    public async IAsyncEnumerable<Result<ICollection<T>>> Select(List<uint> identifiers, uint? parts = null)
    {
        var copy = parts == null ? new List<List<uint>>([identifiers]) : identifiers.SplitIntoFixedParts((int)parts);
        foreach (var part in copy)
        {
            var dic = Sorter.ObtainMultipleItems(part).Where(x => x.Value  != null).Select(x => x.Value!).ToList();

            yield return Res.Value<ICollection<T>>(dic);
        }
    }

    public async IAsyncEnumerable<Result<ICollection<T>>> QueryRange(uint? maximum, List<ICondition>? conditions, uint from = 0, uint? limit = null,
        OrderType order = OrderType.Disordered)
    {
        var list = new List<T>(limit == null ? Sorter.Count : (int)limit);
        await foreach (var res in Query.Query(conditions, limit, order))
        {
            if (!res.TryGetValue(out var rawList, out var error))
            {
                yield return error;
                yield break;
            }
            
            list.Clear();
            list.AddRange(rawList);
                
            if (maximum != null)
            {
                list.RemoveWhere(x => IdentifierGetter(x) > maximum);
            }

            if (from > 0)
            {
                list.RemoveWhere(x => IdentifierGetter(x) < from);
            }

            if (list.Count > 0)
            {
                yield return Res.Value<ICollection<T>>(list);
            }
            
            
        }
    }

    public IAsyncEnumerable<Result<ICollection<T>>> CheckAndSelect(List<uint> identifiers, uint? parts = null)
    {
        throw new NotImplementedException();
    }
}