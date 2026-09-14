using System.Diagnostics.CodeAnalysis;

namespace MaxiNet;

public interface IEntityValidator<T>
{
    bool Validate(T entity, [NotNullWhen(false)] out InvalidationResult<T> error);
}

public static class EntityValidatorExtensions
{
    public static bool ValidateList<T>(this IEntityValidator<T> validator, IEnumerable<T> list,
        [NotNullWhen(false)] out InvalidationResult<T>? error)
    {
        var errorList = new List<InvalidationResult<T>>();

        foreach (var item in list)
            if (!validator.Validate(item, out var itemError))
                errorList.Add(itemError);

        if (errorList.Count == 0)
        {
            error = null;
            return true;
        }

        error = new InvalidationResult<T>(errorList,
            new Oration("The content of this list is invalid because it contains ? invalid items",
                [errorList.Count.ToString()]));
        return false;
    }
}