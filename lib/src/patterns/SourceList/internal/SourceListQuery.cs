namespace MaxiNet;

internal static class SourceListQuery
{
    public static async IAsyncEnumerable<Result<ICollection<T>>> Query<T>(SourceList<T> sl, List<ICondition>? conditions, uint? limit = null,
        OrderType order = OrderType.Disordered)
    {
        if (conditions == null && limit != null && order == OrderType.Disordered)
        {
            yield return Res.Value(sl.Content);
            yield break;
        }

        List<T> orderList = order switch
        {
            OrderType.Disordered => sl.Content.ToList(),
            OrderType.Ascending => sl.Content.OrderBy(sl.IdentifierGetter).ToList(),
            OrderType.Descending => sl.Content.OrderByDescending(sl.IdentifierGetter).ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(order), order, null)
        };

        

        if (conditions != null)
        {
            var result = new List<T>((int?)limit ?? sl.Content.Count);
            foreach (var item in orderList)
            {
                var itsMatch = true;
                foreach (var cond in conditions.OfType<IReferenceCondition>())
                    if (!cond.Execute(item))
                    {
                        itsMatch = false;
                        break;
                    }

                if (itsMatch)
                {
                    result.Add(item);
                    if (limit == null || !(result.Count >= limit)) continue;
                    yield return Res.Value<ICollection<T>>(result);
                    result.Clear();
                }
            }

            if (result.Count <= 0) yield break;
            yield return Res.Value<ICollection<T>>(result);
        }
        else
        {
            if (limit == null)
            {
                yield return  Res.Value<ICollection<T>>(orderList);
            }
            else
            {
                foreach (var parts in
                orderList.SplitIntoFixedParts((int)limit))
                {
                    yield return Res.Value<ICollection<T>>(parts);

                }
            }


        }
        
        
    }
}