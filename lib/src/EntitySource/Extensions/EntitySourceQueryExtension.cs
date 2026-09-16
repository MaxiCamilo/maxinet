namespace MaxiNet;

public static class EntitySourceQueryExtension
{
    public static async Task<Result<T?>> TryObtainItem<T>(this IEntitySourceQuery<T> source, uint  identifier)
    {
        await foreach (var result in source.Select([identifier]))
        {
            if (result.TryGetValue(out var list, out var error))
            {
                return list.Count == 0 ? Res.Value<T?>(default) : Res.Value<T?>(list.First());
            }
            else
            {
                return error.Cast<T?>();
            }
        }

        return Res.Value<T?>(default);
    }

    public static async Task<Result<T>> ObtainItem<T>(this IEntitySourceQuery<T> source, uint identifier)
    {
        if ((await TryObtainItem<T>(source, identifier)).TryGetValue(out var result, out var error))
        {
            return result == null ? Res.ValError<T>(new Oration("Item with identifier ? was not found",[identifier.ToString()])): Res.Value( result);
        }
        else
        {
            return error.Cast<T>();
        }
    }
}