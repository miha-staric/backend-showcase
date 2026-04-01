using MediatR;

public record UpdateCommentCommand(
    Guid Id,
    Guid TenantId,
    Guid TaskId,
    Guid UserId,
    string Subject,
    string Content
) : IRequest<CommentDto?>;
