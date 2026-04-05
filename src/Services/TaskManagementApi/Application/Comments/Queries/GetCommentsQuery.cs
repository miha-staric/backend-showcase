using MediatR;
using TaskManagementApi.Dtos.Comment;

namespace TaskManagementApi.Application.Comments.Queries;

public record GetCommentsQuery() : IRequest<IEnumerable<CommentDto?>>;
