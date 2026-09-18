namespace MaxiNet.EntityMemorySource;

internal class Editor<T> : IEntitySourceEditor<T>
{
    public required IMemoryListSorter<T>  Sorter { get; init; }

    public required Func<T, uint> IdentifierGetter{ get; init; }
    public required Func<T, uint, Result<Nothing>> IdentifierSetter{ get; init; }
    
    public async Task<Result<Nothing>> Aggregate(IAsyncEnumerable<Result<ICollection<T>>> content, bool zeroKeyAutoAssign)
    {
        uint lastId = 0;
        SortedDictionary<uint,T> aggregatedList = new();
        
        await foreach (var resList in content)
        {
            if (!resList.TryGetValue(out var list, out var error))
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
                        return Res.ValError<Nothing>("Item identifier has not been defined");
                    }
                    else if (lastId == 0)
                    {
                        lastId = Sorter.ObtainMax() + 1;
                        
                    }
                    else
                    {
                        lastId += 1;
                    }

                    if (IdentifierSetter(item, lastId).OnError(out var setError)) return setError; 
                }
                else
                {
                    if (!Sorter.Exists(id).TryGetValue(out var exists, out var existsError)) return existsError.Cast<Nothing>();
                    if (exists) return Res.ValError<Nothing>(new Oration("An item with identifier ? already exists",[id.ToString()]));

                    aggregatedList[id] = item;
                }
            }
        }

        return Sorter.SetAll(aggregatedList);
    }

    public async Task<Result<Nothing>> Modifier(IAsyncEnumerable<Result<ICollection<T>>> content)
    {
        SortedDictionary<uint,T> modifierList = new();

        await foreach (var resList in content)
        {
            if (!resList.TryGetValue(out var list, out var error))
            {
                return error.Cast<Nothing>();
            }

            foreach (var item in list)
            {
                var id = IdentifierGetter(item);
                if (id == 0) return Res.ValError<Nothing>("Item identifier has not been defined");
                
                if (!Sorter.Exists(id).TryGetValue(out var exists, out var existsError)) return existsError.Cast<Nothing>();
                if (!exists) return Res.ValError<Nothing>(new Oration("Cannot modify item: identifier ? was not found in the list",[id.ToString()]));
                
                modifierList[id] = item;
            }
        }
        
        return Sorter.SetAll(modifierList);
    }
}