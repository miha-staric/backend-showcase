using MediatR;
using TaskManagementApi.Dtos;

public record GetCommentsQuery() : IRequest<IEnumerable<CommentDto?>>;
