namespace MaxiNet;

public record CompareReferenceeValue(object? Value, ConditionCompareType CompareType = ConditionCompareType.Equal)
    : ICondition, IReferenceCondition
{
    public bool Execute(object? item)
    {
        return new CompareValues(Value, item, CompareType).Execute();
    }
}