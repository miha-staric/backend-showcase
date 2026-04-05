using MediatR;

namespace TaskManagementApi.Application.Comments.Commands;

public record DeleteCommentCommand(Guid CommentId) : IRequest<bool>;
