using MediatR;

namespace TaskManagementApi.Application.Comments.Commands;

public class CommentCreatedEvent(Guid commentId) : INotification
{
    public Guid CommentId { get; } = commentId;
}
