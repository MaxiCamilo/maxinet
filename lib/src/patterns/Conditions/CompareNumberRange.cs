namespace MaxiNet;

public record CompareNumberRange(decimal Number, decimal? Min, decimal? Max) : ICondition, IDirectCondition
{
    public bool Execute()
    {
        if (Min > Number) return false;

        return !(Max < Number);
    }
}