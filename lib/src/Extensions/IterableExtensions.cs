namespace MaxiNet;

public static class IterableExtensions
{
    public static void Lambda<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable) action(item);
    }
    
    public static Result<Nothing> ResultVoidLambda<T>(this IEnumerable<T> enumerable, Func<T, Result<Nothing>> action)
    {
        foreach (var item in enumerable)
        {
            if (action(item).OnError(out var error))
            {
                return error;
            }
        }
        
        return Res.Ok;
    }
    
    public static Result<Nothing> ResultVoidLambda<T>(this IEnumerable<T> enumerable, Action<T> action, Oration? message)
    {
        foreach (var item in enumerable)
        {
            try
            {
                action(item);
            }
            catch (Exception e)
            {
                return new ExceptionResult<Nothing>(e, message ?? new Oration("An error occured"));
            }
        }

        return Res.Ok;
    }
}