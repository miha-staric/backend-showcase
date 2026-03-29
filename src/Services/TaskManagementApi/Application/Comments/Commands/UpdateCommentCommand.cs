using MediatR;

public record UpdateCommentCommand(
    Guid Id,
    Guid TenantId,
    Guid TaskId,
    Guid UserId,
    String Subject,
    String Content
) : IRequest<CommentDto?>;
