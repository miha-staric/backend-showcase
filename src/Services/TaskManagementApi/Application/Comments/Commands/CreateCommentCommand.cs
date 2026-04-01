using MediatR;

public record CreateCommentCommand(Guid TaskId, Guid UserId, string Subject, string Content)
    : IRequest<CommentDto>;
