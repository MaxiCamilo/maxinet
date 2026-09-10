using System;

namespace MaxiNet
{
    public class Lifecycle : Disposable
    {
        private readonly LinkedList<Disposable> _childrens = new LinkedList<Disposable>();
        private readonly LinkedList<Action> _onDisposeActions = new LinkedList<Action>();

        private readonly LinkedList<TemporalLifecycle> _lifeChildrens = new LinkedList<TemporalLifecycle>();

        public Result<Disposable> AddChild(Disposable disposable)
        {
            if (this.ErrorIfDispose() is IFailure failure) return failure.Cast<Disposable>();
            var temp = new TemporalChild(disposable);
            _childrens.AddFirst(temp);
            return Res.Value((Disposable)temp);
        }

        public Result<Nothing> OnDispose(Action action)
        {
            if (this.ErrorIfDispose() is ResultFailure<Nothing> failure) return failure;
            _onDisposeActions.AddFirst(action);
            return Res.Ok;
        }

        public Result<T> Build<T>(Func<T> func) where T : Disposable
        {
            if (this.ErrorIfDispose() is IFailure failure) return failure.Cast<T>();
            var item = func();
            var temp = new TemporalChild(item);
            _childrens.AddFirst(temp);
            return Res.Value(item);
        }

        public Result<Nothing> AttachLifecycle(Lifecycle lifecycle)
        {
            if (this.ErrorIfDispose() is IFailure failure) return failure.Cast<Nothing>();
            if (lifecycle.IsDisposed) return Res.Error("The lifecycle to attach is already disposed");

            _ = new TemporalLifecycle(lifecycle, _lifeChildrens);


            return Res.Ok;
        }

        protected override void PerformDispose()
        {
            foreach (var child in _childrens)
            {
                child.Dispose();
            }
            _childrens.Clear();

            foreach (var action in _onDisposeActions)
            {
                action();
            }
            _onDisposeActions.Clear();

            foreach (var lifecycle in _lifeChildrens)
            {
                lifecycle?.Dispose();
            }
            _lifeChildrens.Clear();

        }



    }

    internal class TemporalChild : Disposable
    {
        private Disposable? item;

        public TemporalChild(Disposable item)
        {
            if (!item.IsDisposed)
            {
                this.item = item;


            }
            else
            {
                this.item = null;
            }
        }

        protected override void PerformDispose()
        {
            item?.Dispose();
            item = null;
        }

    }

    internal class TemporalLifecycle : Disposable
    {
        private Lifecycle _item;
        private LinkedListNode<TemporalLifecycle> _node;
        private LinkedList<TemporalLifecycle> _lifecyclesList;

        public TemporalLifecycle(Lifecycle item, LinkedList<TemporalLifecycle> lifecyclesList)
        {
            _item = item;
            _lifecyclesList = lifecyclesList;
            _node = lifecyclesList.AddFirst(this);


            _item.OnDispose(() =>
            {
                if (IsDisposed) return;
                _lifecyclesList.Remove(_node);
            });

        }

        protected override void PerformDispose()
        {
            _lifecyclesList.Remove(_node);
            _item.Dispose();

        }

    }
}
