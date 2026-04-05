using MediatR;

namespace TaskManagementApi.Application.Comments.Commands;

public class CommentUpdatedEvent(Guid commentId) : INotification
{
    public Guid Id { get; } = commentId;
}
