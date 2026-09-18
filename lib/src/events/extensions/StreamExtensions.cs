using MaxiNet.events.controller;

namespace MaxiNet;

public static class StreamExtensions
{
    public static Result<T> ParalyzeWaitItem<T>(this IStream<T> stream, int? millisecondsTimeout = null,
        CancellationToken? cancellationToken = null)
    {
        if (stream.ErrorIfDispose() is IFailure failure) return failure.Cast<T>();

        var hasElement = false;
        T? element = default;

        var paralize = new ManualResetEventSlim(false);
        var result = stream.Listen(item =>
        {
            hasElement = true;
            element = item;
            paralize.Set();
        }, () => paralize.Set());
        if (result is IFailure listenFailure) return listenFailure.Cast<T>();


        if (millisecondsTimeout != null)
        {
            if (cancellationToken != null)
                paralize.Wait(millisecondsTimeout.Value, cancellationToken.Value);
            else
                paralize.Wait(millisecondsTimeout.Value);
        }
        else
        {
            paralize.Wait();
        }


        stream.Dispose();


        return hasElement ? Res.Value(element!) : Res.ValError<T>("No element received from the stream");
    }

    public static Result<T> ParalyzeWaitItem<T>(this IStreamController<T> controller, int? millisecondsTimeout = null,
        CancellationToken? cancellationToken = null)
    {
        if (controller.ErrorIfDispose() is IFailure failure) return failure.Cast<T>();

        if (controller.BuildStream().TryGetValue(out var stream, out var error))
            return stream.ParalyzeWaitItem(millisecondsTimeout, cancellationToken);

        return error.Cast<T>();
    }

    public static IStream<TR> Map<T, TR>(this IStream<T> stream, Func<T, TR> func) => new StreamReference<T,TR>(){ MainSource = stream, Transform = func };
    
    public static IStream<T> Where<T>(this IStream<T> stream, Predicate<T> predicate) => new StreamReference<T,T>(){ MainSource = stream, Predicate = predicate,Transform = (x) => x };

    public static IStream<TR> WhereType<T,TR>(this IStream<T> stream) where TR : T => new StreamReference<T,TR>(){ MainSource = stream, Predicate = (x) => x is TR,Transform = (x) => (TR)x! };
    

}