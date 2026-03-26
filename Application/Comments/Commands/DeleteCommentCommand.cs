using MediatR;

public record DeleteCommentCommand(Guid CommentId) : IRequest<Boolean>;
