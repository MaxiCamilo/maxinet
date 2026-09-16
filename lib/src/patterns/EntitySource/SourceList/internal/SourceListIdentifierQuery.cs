namespace MaxiNet;

internal static class SourceListIdentifierQuery
{
    public static async IAsyncEnumerable<Result<ICollection<uint>>> QueryIdentifiers<T>(ICollection<T> content ,Func<T, uint> identifierGetter, List<ICondition>? conditions,
        uint? limit = null, OrderType order = OrderType.Disordered)
    {
        if (order == OrderType.Ascending)
        {
            await foreach (var item in QueryIdentifiersAscendant(content, identifierGetter, conditions, limit))
            {
                yield return item;
            }
            yield break;
        }
        
        if (order == OrderType.Descending)
        {
            await foreach (var item in QueryIdentifiersDescending(content, identifierGetter, conditions, limit))
            {
                yield return item;
            }
            yield break;
        }

        var list = new List<uint>();

        foreach (var item in content)
        {
            var id = identifierGetter(item);
            if (conditions != null)
            {
                var itsMatch = true;
                foreach (var cond in conditions.OfType<IReferenceCondition>())
                {
                    if (!cond.Execute(item))
                    {
                        itsMatch = false;
                        break;
                    }
                }
                if (itsMatch)
                {
                    list.Add(id);
                }
                else
                {
                    continue;
                }
                
            }
            else
            {
                list.Add(id);
            }

            if (limit != null && list.Count >= limit)
            {
                yield return Res.Value<ICollection<uint>>(list);
                list.Clear();
            }
            
            
        }

        if (list.Count > 0)
        {
            yield return Res.Value<ICollection<uint>>(list);
        }
        


    }

    private async static IAsyncEnumerable<Result<ICollection<uint>>> QueryIdentifiersAscendant<T>(
        ICollection<T> content ,Func<T, uint> identifierGetter, List<ICondition>? conditions,
        uint? limit = null)
    {
        if (content.Count == 0)
        {
            yield break;
        }
        
        var list = new List<uint>();
        var x = content.Min(identifierGetter);

        foreach (var item in content)
        {
            if (conditions != null)
            {
                var itsMatch = true;
                foreach (var cond in conditions.OfType<IReferenceCondition>())
                {
                    if (!cond.Execute(item))
                    {
                        itsMatch = false;
                        break;
                    }
                }

                if (itsMatch)
                {
                    list.Add(x);
                }
            }
            else
            {
                list.Add(x);
            }

            if (limit != null && list.Count >= limit)
            {
                yield return Res.Value<ICollection<uint>>(list);
                list.Clear();
            }

            uint next = uint.MaxValue;
            foreach (var other in content)
            {
                var id = identifierGetter(other);
                if ( id > x && id < next)
                {
                    next = id;

                }
            }

            if (next == uint.MaxValue)
            {
                break;
            }
        }
        
        if(list.Count > 0)
        {
            yield return Res.Value<ICollection<uint>>(list);
        }
    }
    
    private async static IAsyncEnumerable<Result<ICollection<uint>>> QueryIdentifiersDescending<T>(
        ICollection<T> content ,Func<T, uint> identifierGetter, List<ICondition>? conditions,
        uint? limit = null)
    {
        if (content.Count == 0)
        {
            yield break;
        }
        
        var list = new List<uint>();
        var x = content.Max(identifierGetter);

        foreach (var item in content)
        {
            if (conditions != null)
            {
                var itsMatch = true;
                foreach (var cond in conditions.OfType<IReferenceCondition>())
                {
                    if (!cond.Execute(item))
                    {
                        itsMatch = false;
                        break;
                    }
                }

                if (itsMatch)
                {
                    list.Add(x);
                }
            }
            else
            {
                list.Add(x);
            }

            if (limit != null && list.Count >= limit)
            {
                yield return Res.Value<ICollection<uint>>(list);
                list.Clear();
            }

            uint next = uint.MaxValue;
            foreach (var other in content)
            {
                var id = identifierGetter(other);
                if ( id < x && id > next)
                {
                    next = id;

                }
            }

            if (next == uint.MaxValue)
            {
                break;
            }
        }
        
        if(list.Count > 0)
        {
            yield return Res.Value<ICollection<uint>>(list);
        }
    }
}