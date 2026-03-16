using MediatR;

public class TaskUpdatedEvent : INotification
{
    public Guid TaskId { get; }

    public TaskUpdatedEvent(Guid taskId)
    {
        TaskId = taskId;
    }
}
