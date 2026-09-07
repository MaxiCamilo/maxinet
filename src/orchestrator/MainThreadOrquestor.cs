using System;

namespace MaxiNet
{
    internal class MainThreadOrquestor : Disposable, IThreadOrquestor
    {
        public int Identifier => 0;


        public bool IsRunning => throw new NotImplementedException();

        public bool IsBusy => throw new NotImplementedException();

        public MainThreadOrquestor()
        {

        }

        public async Task<Result<T>> AddExternal<T>(Func<Task<Result<T>>> task)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<T>> AddFunc<T>(Func<Result<T>> func)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<T>> AddTask<T>(Func<Task<Result<T>>> task)
        {
            throw new NotImplementedException();
        }

        



        protected override void PerformDispose()
        {
            throw new NotImplementedException();
        }


    }
}
