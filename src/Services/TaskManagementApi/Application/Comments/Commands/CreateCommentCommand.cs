using MediatR;
using TaskManagementApi.Dtos;

public record CreateCommentCommand(Guid TaskId, Guid UserId, string Subject, string Content)
    : IRequest<CommentDto>;
