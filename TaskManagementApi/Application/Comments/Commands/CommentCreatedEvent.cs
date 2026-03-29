using MediatR;

namespace Contracts;

public class CommentCreatedEvent : INotification
{
    public Guid CommentId { get; }

    public CommentCreatedEvent(Guid commentId)
    {
        CommentId = commentId;
    }
}
