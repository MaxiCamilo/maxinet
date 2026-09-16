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
        catch (Exception ex)
        {
            return Task.FromResult<Result<Nothing>>(new ExceptionResult <Nothing>(ex, new Oration("Source is read-only and cannot be deleted ")));
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

    public async Task<Result<Nothing>> Aggregate(IAsyncEnumerable<Result<ICollection<T>>> content, bool zeroKeyAutoAssign)
    {
        
        await foreach (var part in content)
        {
            uint maxiId = 0;
            if (!part.TryGetValue(out var list, out var error))
            {
                return error.Cast<Nothing>();
            }

            foreach (var item in list)
            {
                var id = IdentifierGetter(item);
                if (id == 0)
                {
                    if (!zeroKeyAutoAssign)
                    {
                        return Res.Error("An item does not have an assigned identifier");
                    }

                    if (maxiId == 0)
                    {
                        maxiId = Content.Count == 0 ? 1 : (Content.Max(IdentifierGetter) + 1);
                    }

                    if (IdentifierSetter(item, maxiId).OnError(out var seterError)) return seterError.Cast<Nothing>();
                    maxiId += 1;
                }
                else
                {
                    foreach (var exists in Content)
                    {
                        if (id == IdentifierGetter(exists))
                        {
                            return Res.Error(new Oration(
                                "Cannot add element with identifier ? because another element has the same identifier.",
                                [id.ToString()]));
                        }

                    }
                }
            }
            
            foreach (var item in list) Content.Add(item);
        }

        return Res.Ok;
    }

    public async Task<Result<Nothing>> Modifier(IAsyncEnumerable<Result<ICollection<T>>> content)
    {
        await foreach (var part in content)
        {
            if (!part.TryGetValue(out var list, out var error))
            {
                return  error.Cast<Nothing>();
            }

            foreach (var item in list)
            {
                var id = IdentifierGetter(item);
                var exists = Content.Any(x => IdentifierGetter(x) == id);
                if (!exists)
                {
                    return Res.Error(new Oration("Cannot perform the modification because the item with identifier %1 was not found",[id.ToString()]));
                }
            }

            foreach (var item in list) Content.Add(item);
        }

        return Res.Ok;
    }


    public Task<Result<uint>> ObtainMaxIdentifier(List<ICondition>? conditions)
    {
        if (Content.Count == 0)
        {
            return Task.FromResult( Res.Value<uint>(0));
        }

        uint maxId = 0;

        foreach (var item in Content)
        {
            var id = IdentifierGetter(item);
            if (id > maxId)
            {
                maxId = id;
            }
        }
        
        return Task.FromResult( Res.Value<uint>(maxId));
    }

    public Task<Result<uint>> ObtainMinIdentifier(List<ICondition>? conditions)
    {
        if (Content.Count == 0)
        {
            return Task.FromResult( Res.Value<uint>(0));
        }

        uint minId = uint.MaxValue;

        foreach (var item in Content)
        {
            var id = IdentifierGetter(item);
            if (id < minId)
            {
                minId = id;
            }
        }
        
        return Task.FromResult( Res.Value<uint>(minId));
    }

    public IAsyncEnumerable<Result<ICollection<uint>>> QueryIdentifiers(List<ICondition>? conditions,
        uint? limit = null, OrderType order = OrderType.Disordered)
        => SourceListIdentifierQuery.QueryIdentifiers(Content, IdentifierGetter, conditions, limit, order);

    public async IAsyncEnumerable<Result<Dictionary<uint, bool>>> CheckAvailability(List<uint>? identifiers,
        List<ICondition>? conditions, uint? limit = null,
        OrderType order = OrderType.Disordered)
    {
        var dictionary = new Dictionary<uint, bool>();
        await foreach (var listResult in Query(conditions, limit, order))
        {
            if (!listResult.TryGetValue(out var list, out var error))
            {
                yield return error.Cast<Dictionary<uint, bool>>();
                yield break;
            }

            foreach (var item in list)
            {
                var id = IdentifierGetter(item);
                dictionary.Add(id, identifiers == null || identifiers.Contains(id));
                if (limit == null || !(dictionary.Count >= limit)) continue;
                yield return Res.Value( dictionary);
                dictionary.Clear();
            }
        }

        if (dictionary.Count > 0)
        {
            yield return Res.Value( dictionary);
        }
    }

    public async IAsyncEnumerable<Result<ICollection<T>>> Select(List<uint> identifiers, uint? parts = null)
    {
        if(identifiers.Count  == 0) yield break;
        
        var list = new List<T>();
        foreach (var id in identifiers)
        {
            var item = Content.FirstOrDefault(x => IdentifierGetter(x) == id);
            if (item == null) continue;
            list.Add(item);
            if (parts == null || !(list.Count >= parts)) continue;
            yield return Res.Value<ICollection<T>>(list);
            list.Clear();
        }

        if (list.Count > 0)
        {
            yield return Res.Value<ICollection<T>>(list);
        }
    }

    public async IAsyncEnumerable<Result<ICollection<T>>> CheckAndSelect(List<uint> identifiers, uint? parts = null)
    {
        if(identifiers.Count  == 0) yield break;
        
        var list = new List<T>();
        foreach (var id in identifiers)
        {
            var item = Content.FirstOrDefault(x => IdentifierGetter(x) == id);
            if (item == null)
            {
                yield return Res.ValError<ICollection<T>>(new Oration("The identifier ? could not be found in the list",[id.ToString()]));
                yield break;
            }
            list.Add(item);
            if (parts == null || !(list.Count >= parts)) continue;
            yield return Res.Value<ICollection<T>>(list);
            list.Clear();
        }

        if (list.Count > 0)
        {
            yield return Res.Value<ICollection<T>>(list);
        }
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
    


    public async Task<Result<Nothing>> DeleteByIdentifier(IAsyncEnumerable<Result<ICollection<uint>>> content)
    {
        var copyList = Content.ToList();
        await foreach (var result in content)
        {
            if (!result.TryGetValue(out var ids, out var error))
            {
                return  error.Cast<Nothing>();
            }

            foreach (var match in ids)
            {
                foreach (var item in copyList)
                {
                    if (IdentifierGetter(item) == match)
                    {
                        Content.Remove(item);
                        break;
                    }
                }
            }
        }
        
        return Res.Ok;
    }


    public Task<Result<Nothing>> DeleteByQuery(List<ICondition> conditions)
    {
        var copyList = Content.ToList();
        
        foreach (var cond in conditions.OfType<IReferenceCondition>())
        {
            int i = 0;
            while (i < copyList.Count)
            {
                var item = copyList[i];
                if (cond.Execute(item))
                {
                    Content.Remove(item);
                }
                else
                {
                    i += 1;
                }
            }
        }
        
        return Task.FromResult(Res.Ok);
    }
}