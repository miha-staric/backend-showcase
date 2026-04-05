using MediatR;
using TaskManagementApi.Dtos.Comment;

namespace TaskManagementApi.Application.Comments.Commands;

public record CreateCommentCommand(Guid TaskId, Guid UserId, string Subject, string Content)
    : IRequest<CommentDto>;
