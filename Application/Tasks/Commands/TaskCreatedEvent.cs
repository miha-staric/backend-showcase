using MediatR;

namespace Contracts;

public class TaskCreatedEvent : INotification
{
    public Guid TaskId { get; }

    public TaskCreatedEvent(Guid taskId)
    {
        TaskId = taskId;
    }
}
