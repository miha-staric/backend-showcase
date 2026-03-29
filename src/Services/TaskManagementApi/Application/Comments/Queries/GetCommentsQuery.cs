using MediatR;

public record GetCommentsQuery() : IRequest<IEnumerable<CommentDto?>>;
