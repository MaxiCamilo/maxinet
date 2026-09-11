using System;
using System.Diagnostics.CodeAnalysis;

namespace MaxiNet;


public abstract class Result<T>
{
    protected Result() { }


    public static implicit operator Result<T>(Func<T> fn)
    {
        return ResultFactory.Try(new Oration("An error occurred"), fn);
    }

    public bool TryGetValue(
        [MaybeNullWhen(false)] out T value,
        [NotNullWhen(false)] out ResultFailure<T>? error)
    {
        if (this is ResultFailure<T> failure)
        {
            value = default;
            error = failure;
            return false;
        }
        else if (this is ResultValue<T> correct)
        {
            value = correct.Content;
            error = null;
            return true;
        }
        else
        {
            value = default;
            error = new NegativeResult<T>(new Oration("Unknown error occurred, this result is neither a success nor a failure"));
            return false;
        }
    }

    public bool OnError([NotNullWhen(true)] out ResultFailure<T>? error)
    {
        if (this is ResultFailure<T> failure)
        {
            error = failure;
            return true;
        }
        else
        {
            error = null;
            return false;
        }
    }

    public bool OnValue([NotNullWhen(true)] out T? value)
    {
        if (this is ResultValue<T> correct)
        {
            value = correct.Content;
            return value != null;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public Result<R> Then<R>(Func<T, Result<R>> func)
    {
        if (this is IFailure failure)
        {
            return failure.Cast<R>();
        }

        else if (this is ResultValue<T> correct)
        {
            return func(correct.Content);
        }
        else
        {
            return new NegativeResult<R>(new Oration("Unknown error occurred, this result is neither a success nor a failure"));
        }
    }

    public Result<R> Select<R>(Func<T, R> func)
    {
        if (this is IFailure failure)
        {
            return failure.Cast<R>();
        }

        else if (this is ResultValue<T> correct)
        {
            var value = func(correct.Content);
            return new ResultValue<R>(value);
        }
        else
        {
            return new NegativeResult<R>(new Oration("Unknown error occurred, this result is neither a success nor a failure"));
        }
    }
}

public static class Res
{
    public static readonly Result<Nothing> Ok = new ResultValue<Nothing>(Nothing.Value);
    public static Result<T> Value<T>(T item) => new ResultValue<T>(item);
    public static Result<Nothing> Error(string message) => new NegativeResult<Nothing>(new Oration(message));
    public static Result<Nothing> Error(Oration message) => new NegativeResult<Nothing>(message);

    public static Result<T> ValError<T>(string message) => new NegativeResult<T>(new Oration(message));
    public static Result<T> ValError<T>(Oration message) => new NegativeResult<T>(message);
}

public interface IFailure
{
    Oration Message { get; }
    Result<R> Cast<R>();
}

public sealed class ResultValue<T>(T content) : Result<T>, IEquatable<ResultValue<T>>
{
    public T Content { get; } = content;
    public override string ToString() => $"Result: {Content}";
    public bool Equals(ResultValue<T>? other) => other is not null && EqualityComparer<T>.Default.Equals(Content, other.Content);
    public override bool Equals(object? obj) => obj is ResultValue<T> other && Equals(other);
    public override int GetHashCode() => Content == null ? 0 : EqualityComparer<T>.Default.GetHashCode(Content);
}


public readonly struct Nothing : IEquatable<Nothing>
{
    public static readonly Nothing Value = default;

    public bool Equals(Nothing other) => true;
    public override bool Equals(object? obj) => obj is Nothing;
    public override int GetHashCode() => 0;
    public override string ToString() => "Nothing";

    public static bool operator ==(Nothing a, Nothing b) => true;
    public static bool operator !=(Nothing a, Nothing b) => false;
}


public abstract class ResultFailure<T> : Result<T>, IFailure
{
    private protected ResultFailure() { }
    public abstract Oration Message { get; }
    public abstract Result<R> Cast<R>();
}

public sealed class NegativeResult<T>(Oration message) : ResultFailure<T>
{
    public override Oration Message { get; } = message;
    public override ResultFailure<R> Cast<R>() => new NegativeResult<R>(Message);
}

public sealed class ExceptionResult<T>(Exception exception, Oration message) : ResultFailure<T>
{
    public Exception Exception { get; } = exception;
    public override Oration Message { get; } = message;

    public override ResultFailure<R> Cast<R>() => new ExceptionResult<R>(Exception, Message);
}

public sealed class CancelationResult<T>(Oration? message = null) : ResultFailure<T>
{

    public static readonly Oration Cancelled = new("The functionality was cancelled");
    public override Oration Message { get; } = message ?? Cancelled;



    public override ResultFailure<R> Cast<R>() => new CancelationResult<R>(Message);
}


public sealed class ResultFactory
{
    static public Result<T> Try<T>(Oration message, Func<T> func)
    {

        try
        {
            return new ResultValue<T>(func());
        }
        catch (OperationCanceledException) { return new CancelationResult<T>(); }
        catch (Exception ex) { return new ExceptionResult<T>(ex, message); }
    }

    static public async Task<Result<T>> TryAsync<T>(Oration message, Func<Task<T>> func)
    {
        try { return new ResultValue<T>(await func().ConfigureAwait(false)); }
        catch (OperationCanceledException) { return new CancelationResult<T>(); }
        catch (Exception ex) { return new ExceptionResult<T>(ex, message); }
    }
}


