namespace MaxiNet;

public static class ListExtensions
{
    public static int BinarySearch<T>(this IList<T> list, T value, IComparer<T>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(list);
        comparer ??= Comparer<T>.Default;

        int lo = 0;
        int hi = list.Count - 1;

        while (lo <= hi)
        {
            int mid = lo + ((hi - lo) >> 1);   // evita overflow de (lo + hi) / 2
            int cmp = comparer.Compare(list[mid], value);

            if (cmp == 0) return mid;
            if (cmp < 0) lo = mid + 1;
            else         hi = mid - 1;
        }

        return ~lo;
    }
    
    public static List<T> ExtractFrom<T>(this IList<T> list, int from, int? amount) {
        if ( list.Count == 0 || from >= list.Count) {
            return [];
        }
        
        
        var newList = new List<T>(amount ?? list.Count - from);
        amount ??= list.Count - from;
        
        int va = 0;

        for (int i = from; i < list.Count; i++) {
            if (va >= amount) {
                break;
            }

            newList.Add(list[i]);
            va += 1;
        }
        return newList;
    }

    public static void RemoveWhere<T>(this IList<T> list, Func<T, bool> predicate)
    {
        int i = 0;
        while (i < list.Count)
        {
            if (predicate(list[i]))
            {
                list.RemoveAt(i);
            }
            else
            {
                i += 1;
            }
        }

    }
}