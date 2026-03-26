using MediatR;

public record UpdateTaskCommand(
    Guid Id,
    Guid TenantId,
    TaskStatus Status,
    string? Title = null,
    string? Description = null,
    Guid? AssignedUserId = null,
    DateTime? DueDate = null
) : IRequest<TaskDto?>;
