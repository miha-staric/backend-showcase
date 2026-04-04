using MediatR;

namespace Contracts;

public class TaskUpdatedEvent(Guid taskId) : INotification
{
    public Guid Id { get; } = taskId;
}
