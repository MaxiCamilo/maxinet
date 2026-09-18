using System.Collections.Immutable;

namespace MaxiNet;

public readonly struct AsyncScope<T> : IDisposable
{
    private readonly AsyncLocal<T>? _local;
    private readonly T _previous;

    internal AsyncScope(AsyncLocal<T> local, T value)
    {
        _local = local;
        _previous = local.Value;
        local.Value = value;
    }

    public void Dispose()
    {
        if (_local is not null)
            _local.Value = _previous;
    }
}

public static class AsyncLocalExtensions
{
    public static AsyncScope<T> Scope<T>(this AsyncLocal<T> local, T value)
        => new(local, value);
}

public sealed class Zone
{
    private static readonly AsyncLocal<Zone?> _current = new();

    public static readonly Zone Root = new(ImmutableDictionary<object, object?>.Empty, null);
    public static Zone Current => _current.Value ?? Root;

    private readonly ImmutableDictionary<object, object?> _values;
    public Zone? Parent { get; }

    private Zone(ImmutableDictionary<object, object?> values, Zone? parent)
    {
        _values = values;
        Parent = parent;
    }

    public bool TryGet<T>(ZoneKey<T> key, out T value)
    {
        if (_values.TryGetValue(key, out var raw) && raw is T t) { value = t; return true; }
        value = default!;
        return false;
    }

    public T Get<T>(ZoneKey<T> key) => TryGet(key, out T v) ? v : key.Default;

    public AsyncScope<Zone?> Fork(params (object Key, object? Value)[] values)
    {
        var builder = _values.ToBuilder();       // O(1), no copia nada todavía
        foreach (var (k, v) in values) builder[k] = v;
        return _current.Scope(new Zone(builder.ToImmutable(), this));
    }
}

public sealed class ZoneKey<T>(string name, T @default = default!)
{
    public string Name => name;
    public T Default => @default;
    public override string ToString() => name;
}