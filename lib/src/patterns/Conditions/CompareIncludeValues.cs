using System.Collections;

namespace MaxiNet;

public record CompareIncludeValues(object Value, IEnumerable Content) : ICondition, IDirectCondition
{
    public bool Execute()
    {
        return Content.Cast<object?>().Contains(Value);
    }
}