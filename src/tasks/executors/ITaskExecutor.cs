using System;

namespace MaxiNet;

public interface ITaskExecutor
{
    //Event new task

    public int PendingTaskCount { get; }

    public bool HasPendingTasks { get; }

    public bool ExecuteTurn();
}

