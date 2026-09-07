using System;

namespace MaxiNet;


public abstract class Result<T>
{
    

    private protected Result() { }


    public static implicit operator Result<T>(Func<T> fn)
    {
        return ResultFactory.Try(new Oration("An error occurred"), fn);
    }
}

public static class Res
{
    public static readonly Result<Nothing> Ok = new ResultValue<Nothing>(Nothing.Value); 
}

public interface IFailure
{
    Oration Message { get; }
    ResultFailure<R> Cast<R>();
}

public sealed class ResultValue<T>(T content) : Result<T>
{

    public T Content { get; } = content;
    public override string ToString() => $"Result: {Content}";
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
    public abstract ResultFailure<R> Cast<R>();
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


