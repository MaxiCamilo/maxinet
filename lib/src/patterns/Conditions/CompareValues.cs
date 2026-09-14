namespace MaxiNet;

public record CompareValues(
    object? First,
    object? Second,
    ConditionCompareType CompareType = ConditionCompareType.Equal) : ICondition, IDirectCondition
{
    public bool Execute()
    {
        return CompareType switch
        {
            ConditionCompareType.Equal => First == Second,
            ConditionCompareType.NotEqual => First != Second,
            ConditionCompareType.Greater => First is IComparable fc && Second is IComparable sc && fc.CompareTo(sc) > 0,
            ConditionCompareType.Less => First is IComparable fc && Second is IComparable sc && fc.CompareTo(sc) < 0,
            ConditionCompareType.GreaterEqual => First is IComparable fc && Second is IComparable sc &&
                                                 fc.CompareTo(sc) >= 0,
            ConditionCompareType.LessEqual => First is IComparable fc && Second is IComparable sc &&
                                              fc.CompareTo(sc) <= 0,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}