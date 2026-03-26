using MediatR;

public record CreateCommentCommand(Guid TaskId, Guid UserId, String Subject, String Content)
    : IRequest<CommentDto>;
