using MediatR;
using TaskManagementApi.Dtos;

public record GetCommentByIdQuery(Guid CommentId) : IRequest<CommentDto?>;
