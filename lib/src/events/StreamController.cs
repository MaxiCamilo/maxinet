using System;
using MaxiNet.events.controller;

namespace MaxiNet;

public static class StreamController<T>
{
    public static IStreamController<T> Sync() => new SyncStreamController<T>();
    public static IStreamController<T> ThreadSafe() => new ThreadSafeStreamController<T>();
}



