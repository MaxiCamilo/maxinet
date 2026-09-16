namespace MaxiNet;

public record CompareIdentifier(Func<uint, bool> Comparator) : ICondition
{
    public static CompareIdentifier Match(uint value, ConditionCompareType compareType = ConditionCompareType.Equal)
    {
        return new CompareIdentifier((id) => new CompareValues(First: id, Second: value, compareType).Execute() );
    }
    
    
    
    public bool Execute(uint identifier)
    {
        return Comparator(identifier);
    }
    
    
}