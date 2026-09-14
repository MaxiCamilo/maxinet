namespace MaxiNet;

public static class EnumerableExtensions
{
    public static IEnumerable<List<T>> SplitIntoFixedParts<T>(this IEnumerable<T> source, int size)
    {
        var buffer = new List<T>(size);
        foreach (var item in source)
        {
            buffer.Add(item);
            if (buffer.Count == size)
            {
                yield return buffer;
                buffer = new List<T>(size);
            }
        }
        if (buffer.Count > 0) yield return buffer;
    }
}