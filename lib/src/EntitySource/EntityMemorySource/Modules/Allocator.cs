namespace MaxiNet.EntityMemorySource;

internal class Allocator<T> : IAllocateSource<T>
{
    public required IMemoryListSorter<T>  Sorter { get; init; }
    public required Func<T, uint> IdentifierGetter{ get; init; }
    
    
    public async Task<Result<Nothing>> Allocate(IAsyncEnumerable<Result<ICollection<T>>> content)
    {
        await foreach (var resList in content)
        {
            if (resList.TryGetValue(out var list, out var res))
            {
                Sorter.SetAll(list.ToDictionary((x) => IdentifierGetter(x)));
            }
            else
            {
                return res.Cast<Nothing>();
            }
        }

        return Res.Ok;
    }
}