using MediatR;
using TaskManagementApi.Dtos.Comment;

namespace TaskManagementApi.Application.Comments.Queries;

public record GetCommentByIdQuery(Guid CommentId) : IRequest<CommentDto?>;
