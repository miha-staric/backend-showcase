using MediatR;

namespace Contracts;

public class TaskUpdatedEvent : INotification
{
    public Guid Id { get; }

    public TaskUpdatedEvent(Guid taskId)
    {
        Id = taskId;
    }
}
