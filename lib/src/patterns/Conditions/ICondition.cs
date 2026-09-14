namespace MaxiNet;

public enum ConditionCompareType
{
    Equal,
    NotEqual,
    Greater,
    Less,
    GreaterEqual,
    LessEqual
}

public interface ICondition
{
}

public interface IDirectCondition
{
    bool Execute();
}

public interface IReferenceCondition
{
    bool Execute(object? item);
}