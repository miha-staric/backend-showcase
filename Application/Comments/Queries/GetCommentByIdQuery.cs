using MediatR;

public record GetCommentByIdQuery(Guid CommentId) : IRequest<CommentDto?>;
