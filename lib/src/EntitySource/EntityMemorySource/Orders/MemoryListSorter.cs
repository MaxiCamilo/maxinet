using System.Diagnostics.CodeAnalysis;

namespace MaxiNet;

internal class MemoryListSorter<T> : IMemoryListSorter<T>
{
    
    private readonly IDictionary<uint,T> _memoryInstance;

    private MemoryListSorter(IDictionary<uint,T> instance)
    {
        _memoryInstance  = instance;
    }

    public static MemoryListSorter<T> SortedList()
    {
        return new MemoryListSorter<T>(new SortedList<uint,T>());
    }
    
    public static MemoryListSorter<T> SortedList(SortedList<uint,T> list)
    {
        return new MemoryListSorter<T>(list);
    }

    public static MemoryListSorter<T> Linked()
    {
        return new MemoryListSorter<T>(new SortedDictionary<uint,T>());
    }
    
    public static MemoryListSorter<T> Linked(SortedDictionary<uint,T> link)
    {
        return new MemoryListSorter<T>(link);
    }
    
    public int Count => _memoryInstance.Count;


    public Result<Nothing> Set(uint id, T item)
    {
        _memoryInstance[id] = item;
        return Res.Ok;
    }

    public Result<Nothing> SetAll(IDictionary<uint, T> values)
    {
        foreach (var pair in values)
        {
            _memoryInstance[pair.Key] = pair.Value;
        }

        return Res.Ok;
    }

    public Result<Nothing> RemoveAll()
    {
        _memoryInstance.Clear();
        return Res.Ok;
    }

    public Result<Nothing> Remove(uint id)
    {
        _memoryInstance.Remove(id);
        return Res.Ok;
    }

    public Result<Nothing> RemoveAll(IEnumerable<uint> ids)
    {
        foreach (var id in ids)
        {
            _memoryInstance.Remove(id);
        }

        return Res.Ok;
    }

    public Result<bool> Exists(uint id)
    {
        return Res.Value(_memoryInstance.ContainsKey(id));
    }

    public IEnumerable<KeyValuePair<uint, bool>> Exists(IEnumerable<uint> ids)
    {
        foreach (var id in ids)
        {
            yield return new KeyValuePair<uint, bool>(id,_memoryInstance.ContainsKey(id));
        }
    }



    public bool Obtain(uint id, [NotNullWhen(true)] out T? item)
    {
        if (_memoryInstance.TryGetValue(id, out var reference))
        {
            item = reference!;
            return true;
        }
        else
        {
            item = default;
            return  false;
        }
    }

    public Result<ICollection<T>> ObtainAll(IEnumerable<uint> ids)
    {
        var list = new LinkedList<T>();

        foreach(var id in ids)
        {
            if (Obtain(id, out var item))
            {
                list.AddLast(item);
            }
        }

        return Res.Value<ICollection<T>>(list);
    }

    public IEnumerable<KeyValuePair<uint, T>> AscendingEnumerable(uint from = 0)
    {
        if (_memoryInstance.Count == 0)
        {
            yield break;
        }

        if (from == 0)
        {
            foreach (var val in _memoryInstance)
            {
                yield return val;
            }

            yield break;
        }
        
        if (_memoryInstance.Count == 1)
        {
            var first = _memoryInstance.Keys.First();
            if (first >= from)
            {
                yield return new KeyValuePair<uint, T>(first, _memoryInstance.Values.First());
                yield break;
            }
        }

        if (_memoryInstance is SortedList<uint, T> list)
        {
            foreach (var item in AscendingEnumerable(list,from))
            {
                yield return item;
            }
            yield break;
        }


        foreach (var pair in _memoryInstance)
        {
            if (pair.Key >= from)
            {
                yield return pair;
            }
        }
    }

    private IEnumerable<KeyValuePair<uint, T>> AscendingEnumerable(SortedList<uint, T> list, uint from)
    {
        var position = list.Keys.BinarySearch(from);
        if (position < 0)
        {
            position = ~position;
            if (list.Keys[position] < from)
            {
                position += 1;
            }
        }

        for (int i = position; i <= list.Keys.Count; i++)
        {
            yield return new KeyValuePair<uint, T>(list.Keys[i], list.Values[i]);
        }
    }

    public IEnumerable< KeyValuePair<uint,T>> DescendingEnumerable(uint from = 0)
    {
        if (_memoryInstance.Count == 0)
        {
            yield break;
        }

        if (from == 0)
        {
            foreach (var val in _memoryInstance)
            {
                yield return val;
            }

            yield break;
        }
        
        if (_memoryInstance.Count == 1)
        {
            var first = _memoryInstance.Keys.First();
            if (first <= from)
            {
                yield return new KeyValuePair<uint, T>(first, _memoryInstance.Values.First());
                yield break;
            }
        }
        
        
        if (_memoryInstance is SortedList<uint, T> list)
        {
            foreach (var item in DescendingEnumerable(list,from))
            {
                yield return item;
            }
            yield break;
        }
        
        
        foreach (var pair in _memoryInstance.Reverse())
        {
            if (pair.Key <= from)
            {
                yield return pair;
            }
        }
    }

    public uint ObtainMax()
    {
        if (_memoryInstance.Count == 0)
        {
            return 0;
        }

        if (_memoryInstance is SortedList<uint, T> shorted)
        {
            return shorted.Keys[shorted.Count - 1];
        }
        else
        {
            return _memoryInstance.Last().Key;
        }
    }

    public uint ObtainMin()
    {
        if (_memoryInstance.Count == 0)
        {
            return 0;
        }

        if (_memoryInstance is SortedList<uint, T> shorted)
        {
            return shorted.Keys[0];
        }
        else
        {
            return _memoryInstance.First().Key;
        }
    }

    public T? ObtainItem(uint id)
    {
        if (_memoryInstance.Count == 0) return default;

        if (_memoryInstance is SortedList<uint, T> list)
        {
            var position = list.Keys.BinarySearch(id);
            return position < 0 ? default : list.Values[position];
        }

        foreach (var pair in _memoryInstance)
        {
            if (pair.Key == id)
            {
                return pair.Value;
            }
        }
        
        return  default;
    }
    
    public IEnumerable<KeyValuePair<uint, T?>> ObtainMultipleItems(List<uint> ids)
    {
        
        if (_memoryInstance.Count == 0 || ids.Count == 0) yield break;

        var identifier = ids.ToList();
        identifier.Sort();

        if (_memoryInstance is SortedList<uint, T> list)
        {
            var position = list.Keys.BinarySearch(identifier[0]);
            if(position < 0)
            {
                yield return new KeyValuePair<uint, T?>(identifier[0], default);
                yield break;
            }
            else
            {
                yield return new KeyValuePair<uint, T?>(identifier[0], list.Values[position]);
            }

            for (int i = 1; i < identifier.Count; i++)
            {
                position = list.Keys.BinarySearch(identifier[i]);
                if (position < 0)
                {
                    yield return new KeyValuePair<uint, T?>(identifier[i], default);
                }
                else
                {
                    yield return new KeyValuePair<uint, T?>(identifier[i], list.Values[position]);
                }
            }
        }

        foreach (KeyValuePair<uint, T> pair in _memoryInstance)
        {
            T? item = default;
            foreach (var id in ids)
            {
                if (pair.Key == id)
                {
                    item = pair.Value;
                    break;
                }
            }

            yield return new KeyValuePair<uint, T?>(pair.Key, item);
        }
        
        
    }

    private IEnumerable<KeyValuePair<uint, T>> DescendingEnumerable(SortedList<uint, T> list, uint from)
    {
        var position = list.Keys.BinarySearch(from);
        if (position < 0)
        {
            position = ~position;
            if (list.Keys[position] > from)
            {
                position -= 1;
            }
        }

        for (int i = position; i >= 0; i--)
        {
            yield return new KeyValuePair<uint, T>(list.Keys[i], list.Values[i]);
        }
    }
}