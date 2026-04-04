using MediatR;

namespace Contracts;

public class TaskCreatedEvent(Guid taskId) : INotification
{
    public Guid TaskId { get; } = taskId;
}
