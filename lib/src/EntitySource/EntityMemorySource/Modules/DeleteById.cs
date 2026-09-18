namespace MaxiNet.EntityMemorySource;

internal class DeleteById<T> : ISourceDeleteByIdentifier
{
    public required IMemoryListSorter<T>  Sorter { get; init; }
    public required Func<T, uint> IdentifierGetter{ get; init; }
    
    public async Task<Result<Nothing>> DeleteByIdentifier(IAsyncEnumerable<Result<ICollection<uint>>> content)
    {
        if (Sorter.Count == 0) return Res.Ok;

        LinkedList<uint> ids = new();

        await foreach (var resList in content)
        {
            if (!resList.TryGetValue(out var list, out var error))
            {
                return error.Cast<Nothing>();
            }

            foreach (var id in list)
            {
                if (!Sorter.Exists(id).TryGetValue(out var exists, out var exitsError))
                    return exitsError.Cast<Nothing>();

                if (exists)
                {
                    ids.AddLast(id);
                }
            }
        }
        
        return  Sorter.RemoveWhere(x => ids.Contains(IdentifierGetter(x)));
    }
}