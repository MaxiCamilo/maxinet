namespace MaxiNet;

public class EntityMemorySource<T>: IEntityStorage<T>
{
    public IQuerySource<T> Query { get; }
    public IAllocateSource<T> Allocate { get; }
    public ICompleteDeleteSource<T> CompleteDelete { get; }
    public IEntitySource<T> Source { get; }
    public IEntitySourceQuery<T> SourceQuery { get; }
    public ISourceDeleteByQuery DeleteByQuery { get; }
    public ISourceDeleteByIdentifier DeleteByIdentifier { get; }
    public IEntitySourceEditor<T> Editor { get; }
}