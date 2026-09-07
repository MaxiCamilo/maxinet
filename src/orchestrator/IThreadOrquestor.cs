using System;

namespace MaxiNet;

public interface IThreadOrquestor : IDisposable
{
    public int Identifier { get; }
    public bool IsRunning { get; }

    public bool IsBusy { get; }

    public Task<Result<T>> AddFunc<T>(Func<Result<T>> func);
    public Task<Result<T>> AddExternal<T>(Func<Task<Result<T>>> task);
    public Task<Result<T>> AddTask<T>(Func<Task<Result<T>>> task);

}

