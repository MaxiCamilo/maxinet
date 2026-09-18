namespace MaxiNet.EntityMemorySource;

internal class DeleteByQueryImpl<T> : ISourceDeleteByQuery
{
    public required IMemoryListSorter<T>  Sorter { get; init; }
    public required Func<T, uint> IdentifierGetter{ get; init; }
    
    public async Task<Result<Nothing>> DeleteByQuery(List<ICondition> conditions)
    {
        if (conditions.Count == 0 || Sorter.Count == 0)
        {
            return Res.Ok;
        }
        
        var conditioner = new SourceConditioner<T>()
        {
            IdentifierGetter = IdentifierGetter,
            Conditions = conditions,
        };

        Sorter.RemoveWhere(conditioner.Execute);
        
        return Res.Ok;
    }
}