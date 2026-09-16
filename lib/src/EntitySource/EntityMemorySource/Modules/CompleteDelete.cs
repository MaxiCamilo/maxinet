namespace MaxiNet.EntityMemorySource;

internal class CompleteDelete<T> : ICompleteDeleteSource<T>
{
    public required IMemoryListSorter<T>  Sorter { get; init; }
    public async Task<Result<Nothing>> DeleteAll()
    {
        return Sorter.RemoveAll();
    }
}