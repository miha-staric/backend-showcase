using MediatR;

namespace Contracts;

public class TaskDeletedEvent(Guid taskId) : INotification
{
    public Guid TaskId { get; } = taskId;
}
