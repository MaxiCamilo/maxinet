using MaxiNet.events.controller;

namespace MaxiNet;

public static class StreamController<T>
{
    public static IStreamController<T> Sync()
    {
        return new SyncStreamController<T>();
    }

    public static IStreamController<T> ThreadSafe()
    {
        return new ThreadSafeStreamController<T>();
    }
}