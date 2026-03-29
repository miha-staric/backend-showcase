using MediatR;

namespace Contracts;

public class CommentUpdatedEvent : INotification
{
    public Guid Id { get; }

    public CommentUpdatedEvent(Guid commentId)
    {
        Id = commentId;
    }
}
