namespace MaxiNet.EntityMemorySource;

internal class SourceConfig<T> : IEntitySource<T>
{
    
    public required string PrimaryKey { get; init; }
    
    public required Func<T, uint> IdentifierGetter { get; init; }
    
    public required Func<T, uint, Result<Nothing>> IdentifierSetter{ get; init; }

    public uint ObtainIdentifier(T entity) => IdentifierGetter(entity);

    public Result<Nothing> ChangeIdentifier(T entity, uint identifier) => IdentifierSetter(entity, identifier);
}