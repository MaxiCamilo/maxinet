using MaxiNet.EntityMemorySource;

namespace MaxiNet;

public class EntityMemorySource<T>: IEntityStorage<T>
{
    public IQuerySource<T> Query { get; }
    public IAllocateSource<T> Allocate { get; }
    public ICompleteDeleteSource<T> CompleteDelete { get; }
    public IEntitySource<T> SourceConfig { get; }
    public IEntitySourceQuery<T> EntityQuery { get; }
    public ISourceDeleteByQuery DeleteByQuery { get; }
    public ISourceDeleteByIdentifier DeleteByIdentifier { get; }
    public IEntitySourceEditor<T> Editor { get; }


    private EntityMemorySource(IMemoryListSorter<T> sorter, Func<T, uint> identifierGetter, Func<T, uint, Result<Nothing>> identifierSetter, string primaryKey = "")
    {
        Query = new QuerySource<T>{IdentifierGetter = identifierGetter, Sorter = sorter};
        Allocate = new Allocator<T> { IdentifierGetter = identifierGetter, Sorter = sorter };
        CompleteDelete = new CompleteDelete<T> { Sorter = sorter };
        SourceConfig = new SourceConfig<T>
            { IdentifierGetter = identifierGetter, IdentifierSetter = identifierSetter, PrimaryKey = primaryKey };
        EntityQuery = new EntitySourceQuery<T> {IdentifierGetter = identifierGetter, Query = Query, Sorter = sorter};
        DeleteByQuery = new DeleteByQueryImpl<T> { Sorter = sorter, IdentifierGetter = identifierGetter };
        DeleteByIdentifier = new DeleteById<T>{ IdentifierGetter = identifierGetter, Sorter = sorter};
        Editor = new Editor<T>{ IdentifierGetter = identifierGetter, IdentifierSetter = identifierSetter, Sorter = sorter };
    }

    static EntityMemorySource<T> SortedList(Func<T, uint> identifierGetter, Func<T, uint, Result<Nothing>> identifierSetter) =>new EntityMemorySource<T>(MemoryListSorter<T>.SortedList(), identifierGetter, identifierSetter);
    static EntityMemorySource<T> SortedList(SortedList<uint,T> list,Func<T, uint> identifierGetter, Func<T, uint, Result<Nothing>> identifierSetter) =>new EntityMemorySource<T>(MemoryListSorter<T>.SortedList(list),identifierGetter, identifierSetter);

    static EntityMemorySource<T> SortedList(ICollection<T> list, Func<T, uint> identifierGetter,
        Func<T, uint, Result<Nothing>> identifierSetter)
    {
        var sorted = new SortedList<uint, T>(list.Count);
        foreach (var item in list)
        {
            sorted[identifierGetter(item)] = item;
        }
        
        return SortedList(sorted, identifierGetter, identifierSetter);
    }
    
    static EntityMemorySource<T> SortedList(IEnumerable<T> list, Func<T, uint> identifierGetter,
        Func<T, uint, Result<Nothing>> identifierSetter)
    {
        var sorted = new SortedList<uint, T>();
        foreach (var item in list)
        {
            sorted[identifierGetter(item)] = item;
        }
        
        return SortedList(sorted, identifierGetter, identifierSetter);
    }
    
    static EntityMemorySource<T> Linked(Func<T, uint> identifierGetter, Func<T, uint, Result<Nothing>> identifierSetter)=>new EntityMemorySource<T>(MemoryListSorter<T>.Linked(),identifierGetter, identifierSetter );
    static EntityMemorySource<T> Linked(SortedDictionary<uint,T> link,Func<T, uint> identifierGetter, Func<T, uint, Result<Nothing>> identifierSetter)=>new EntityMemorySource<T>(MemoryListSorter<T>.Linked(link),identifierGetter, identifierSetter );

    
    static EntityMemorySource<T> Linked(IEnumerable<T> collection, Func<T, uint> identifierGetter,
        Func<T, uint, Result<Nothing>> identifierSetter)
    {
        var link = new SortedDictionary<uint, T>();
        foreach (var item in collection)
        {
            link[identifierGetter(item)] = item;
        }
        
        return Linked(link, identifierGetter, identifierSetter);
    }

}