using System;

namespace MaxiNet;

public static class StreamExtensions
{
    public static Result<T> ParalyzeWaitItem<T>(this IStream<T> stream, int? millisecondsTimeout = null, CancellationToken? cancellationToken = null)
    {
        if (stream.ErrorIfDispose() is IFailure failure) return failure.Cast<T>();

        bool hasElement = false;
        T? element = default;

        var paralize = new ManualResetEventSlim(false);
        var result = stream.Listen((item) =>
        {

            hasElement = true;
            element = item;
            paralize.Set();
        }, () => paralize.Set());
        if (result is IFailure listenFailure) return listenFailure.Cast<T>();



        if (millisecondsTimeout != null)
        {
            if (cancellationToken != null)
            {
                paralize.Wait(millisecondsTimeout.Value, cancellationToken.Value);
            }
            else
            {
                paralize.Wait(millisecondsTimeout.Value);
            }
        }
        else
        {

            paralize.Wait();
        }


        stream.Dispose();


        return hasElement ? Res.Value(element!) : Res.ValError<T>("No element received from the stream");
    }

    public static Result<T> ParalyzeWaitItem<T>(this IStreamController<T> controller, int? millisecondsTimeout = null, CancellationToken? cancellationToken = null)
    {
        if (controller.ErrorIfDispose() is IFailure failure) return failure.Cast<T>();

        if (controller.BuildStream().TryGetValue(out var stream, out var error))
        {
            return stream.ParalyzeWaitItem(millisecondsTimeout, cancellationToken);
        }
        else
        {
            return error.Cast<T>();
        }

    }

}

