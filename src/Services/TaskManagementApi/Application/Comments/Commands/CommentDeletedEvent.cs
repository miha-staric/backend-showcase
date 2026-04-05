using MediatR;

namespace TaskManagementApi.Application.Comments.Commands;

public class CommentDeletedEvent(Guid commentId) : INotification
{
    public Guid CommentId { get; } = commentId;
}
