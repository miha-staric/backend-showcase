using MediatR;

namespace Contracts;

public class CommentDeletedEvent : INotification
{
    public Guid CommentId { get; }

    public CommentDeletedEvent(Guid commentId)
    {
        CommentId = commentId;
    }
}
