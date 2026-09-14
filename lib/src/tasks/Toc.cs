namespace MaxiNet;

public static class Toc
{
    private static ITocInstance Instance { get; set; } = new FirstInstance();

    public static Task<Result<Nothing>> Ruminate(Action action)
    {
        return Instance.Add(action);
    }


    public static Task<Result<T>> Ruminate<T>(Func<T> function)
    {
        return Instance.Add(function);
    }

    public static Task<Result<T>> Ruminate<T>(Func<Task<T>> function)
    {
        return Instance.Add(function);
    }

    public static Task<Result<T>> Ruminate<T>(Func<Task<Result<T>>> function)
    {
        return Instance.Add(function);
    }

    private class FirstInstance : ITocInstance
    {
        public async Task<Result<Nothing>> Add(Action action)
        {
            if (Build().TryGetValue(out var instance, out var error)) return await instance.Add(action);

            return error.Cast<Nothing>();
        }

        public async Task<Result<T>> Add<T>(Func<T> function)
        {
            if (Build().TryGetValue(out var instance, out var error)) return await instance.Add(function);

            return error.Cast<T>();
        }

        public async Task<Result<T>> Add<T>(Func<Task<T>> function)
        {
            if (Build().TryGetValue(out var instance, out var error)) return await instance.Add(function);

            return error.Cast<T>();
        }

        public async Task<Result<T>> Add<T>(Func<Task<Result<T>>> function)
        {
            if (Build().TryGetValue(out var instance, out var error)) return await instance.Add(function);

            return error.Cast<T>();
        }

        private Result<TocInstance> Build()
        {
            var newInstance = new TocInstance();

            if (newInstance.Initialize().OnError(out var error))
                return error.Cast<TocInstance>();

            Instance = newInstance;
            return Res.Value(newInstance);
        }
    }
}