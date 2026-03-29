using MediatR;

namespace Contracts;

public class TaskDeletedEvent : INotification
{
    public Guid TaskId { get; }

    public TaskDeletedEvent(Guid taskId)
    {
        TaskId = taskId;
    }
}
