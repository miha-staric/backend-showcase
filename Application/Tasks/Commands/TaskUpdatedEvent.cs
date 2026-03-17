using MediatR;

namespace Contracts;

public class TaskUpdatedEvent : INotification
{
    public Guid TaskId { get; }

    public TaskUpdatedEvent(Guid taskId)
    {
        TaskId = taskId;
    }
}
