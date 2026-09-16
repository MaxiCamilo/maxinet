namespace MaxiNet;

public record CompareReferenceValue(object? Value, ConditionCompareType CompareType = ConditionCompareType.Equal)
    : ICondition, IReferenceCondition
{
    public bool Execute(object? item)
    {
        return new CompareValues(Value, item, CompareType).Execute();
    }
}