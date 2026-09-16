namespace MaxiNet.EntityMemorySource;

internal class SourceConditioner<T>
{
    public required Func<T, uint> IdentifierGetter{ get; init; }
    public required IEnumerable<ICondition> Conditions{ get; init; }

    public bool Execute(T item)
    {
        foreach (var rawCondition in Conditions)
        {
            bool match = false;
            if (rawCondition is IDirectCondition direct)
            {
                match = direct.Execute();
            }
            else if (rawCondition is CompareIdentifier compId)
            {
                match = compId.Execute(IdentifierGetter(item));
            }

            if (!match)
            {
                return false;
            }
        }

        return true;
    }
}