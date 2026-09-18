using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace MaxiNet;

public abstract class Result<T>
{

    public bool TryGetValue(
        [MaybeNullWhen(false)] out T value,
        [NotNullWhen(false)] out ResultFailure<T>? error)
    {
        switch (this)
        {
            case ResultFailure<T> failure:
                value = default;
                error = failure;
                return false;
            case ResultValue<T> correct:
                value = correct.Content;
                error = null;
                return true;
            default:
                value = default;
                error = new NegativeResult<T>(
                    new Oration("Unknown error occurred, this result is neither a success nor a failure"));
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

        error = null;
        return false;
    }

    public bool OnValue([NotNullWhen(true)] out T? value)
    {
        if (this is ResultValue<T> correct)
        {
            value = correct.Content;
            return value != null;
        }

        value = default;
        return false;
    }

    public Result<TR> Then<TR>(Func<T, Result<TR>> func)
    {
        return this switch
        {
            IFailure failure => failure.Cast<TR>(),
            ResultValue<T> correct => func(correct.Content),
            _ => new NegativeResult<TR>(
                new Oration("Unknown error occurred, this result is neither a success nor a failure"))
        };
    }

    public Result<TR> Select<TR>(Func<T, TR> func)
    {
        switch (this)
        {
            case IFailure failure:
                return failure.Cast<TR>();
            case ResultValue<T> correct:
            {
                var value = func(correct.Content);
                return new ResultValue<TR>(value);
            }
            default:
                return new NegativeResult<TR>(
                    new Oration("Unknown error occurred, this result is neither a success nor a failure"));
        }
    }
}

public static class Res
{
    public static readonly Result<Nothing> Ok = new ResultValue<Nothing>(Nothing.Value);

    public static Result<T> Value<T>(T item)
    {
        return new ResultValue<T>(item);
    }

    public static Result<Nothing> Error(string message)
    {
        return new NegativeResult<Nothing>(new Oration(message));
    }

    public static Result<Nothing> Error(Oration message)
    {
        return new NegativeResult<Nothing>(message);
    }

    public static Result<T> ValError<T>(string message)
    {
        return new NegativeResult<T>(new Oration(message));
    }

    public static Result<T> ValError<T>(Oration message)
    {
        return new NegativeResult<T>(message);
    }
    
    public static Result<Nothing> Volatile(Oration message, Action func)
    {
        try
        {
            func();
            return Res.Ok;
        }
        catch (OperationCanceledException)
        {
            return new CancellationResult<Nothing>();
        }
        catch (Exception e)
        {
            return new ExceptionResult<Nothing>(e, message);
        }
    }

    public static Result<T> Volatile<T>(Oration message, Func<T> func)
    {
        try
        {
            return new ResultValue<T>(func());
        }
        catch (OperationCanceledException)
        {
            return new CancellationResult<T>();
        }
        catch (Exception e)
        {
            return new ExceptionResult<T>(e, message);
        }
    }

    public static async Task<Result<T>> AsyncVolatile<T>(Oration message, Func<Task<T>> func)
    {
        try
        {
            var result = await func();
            return new ResultValue<T>(result);
        }
        catch (OperationCanceledException)
        {
            return new CancellationResult<T>();
        }
        catch (Exception e)
        {
            return new ExceptionResult<T>(e, message);
        }
    }
}

public interface IFailure
{
    Oration Message { get; }
    Result<TR> Cast<TR>();
}

public sealed class ResultValue<T>(T content) : Result<T>, IEquatable<ResultValue<T>>
{
    public T Content { get; } = content;

    public bool Equals(ResultValue<T>? other)
    {
        return other is not null && EqualityComparer<T>.Default.Equals(Content, other.Content);
    }

    public override string ToString()
    {
        return $"Result: {Content}";
    }

    public override bool Equals(object? obj)
    {
        return obj is ResultValue<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Content == null ? 0 : EqualityComparer<T>.Default.GetHashCode(Content);
    }
}

public readonly struct Nothing : IEquatable<Nothing>
{
    public static readonly Nothing Value = default;

    public bool Equals(Nothing other)
    {
        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is Nothing;
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public override string ToString()
    {
        return "Nothing";
    }

    public static bool operator ==(Nothing a, Nothing b)
    {
        return true;
    }

    public static bool operator !=(Nothing a, Nothing b)
    {
        return false;
    }
}

public abstract class ResultFailure<T> : Result<T>, IFailure
{
    public abstract Oration Message { get; }
    public abstract Result<TR> Cast<TR>();
}

public sealed class NegativeResult<T>(Oration message) : ResultFailure<T>
{
    public override Oration Message { get; } = message;

    public override ResultFailure<TR> Cast<TR>()
    {
        return new NegativeResult<TR>(Message);
    }
}

public sealed class InvalidationResult<T>(IReadOnlyList<IFailure> errors, Oration? message = null) : ResultFailure<T>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Oration DefaultValue = new("Invalid data found");
    public override Oration Message { get; } = message ?? DefaultValue;

    public IReadOnlyList<IFailure> Errors => errors;

    public override ResultFailure<TR> Cast<TR>()
    {
        return new InvalidationResult<TR>(Errors, message);
    }
}

public sealed class ExceptionResult<T>(Exception exception, Oration message) : ResultFailure<T>
{
    // ReSharper disable once MemberCanBePrivate.Global
    public Exception Exception => exception;
    public override Oration Message { get; } = message;

    public override ResultFailure<TR> Cast<TR>()
    {
        return new ExceptionResult<TR>(Exception, Message);
    }
}

public sealed class CancellationResult<T>(Oration? message = null) : ResultFailure<T>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Oration Cancelled = new("The functionality was cancelled");
    public override Oration Message { get; } = message ?? Cancelled;


    public override ResultFailure<TR> Cast<TR>()
    {
        return new CancellationResult<TR>(Message);
    }
}

public static class ResultFactory
{
    public static Result<T> Try<T>(Oration message, Func<T> func)
    {
        try
        {
            return new ResultValue<T>(func());
        }
        catch (OperationCanceledException)
        {
            return new CancellationResult<T>();
        }
        catch (Exception ex)
        {
            return new ExceptionResult<T>(ex, message);
        }
    }

    public static async Task<Result<T>> TryAsync<T>(Oration message, Func<Task<T>> func)
    {
        try
        {
            return new ResultValue<T>(await func().ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            return new CancellationResult<T>();
        }
        catch (Exception ex)
        {
            return new ExceptionResult<T>(ex, message);
        }
    }
}