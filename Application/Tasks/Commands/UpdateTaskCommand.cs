using MediatR;

public record UpdateTaskCommand(
    Guid TaskId,
    Guid TenantId,
    TaskStatus Status,
    string? Title = null,
    string? Description = null,
    Guid? AssignedUserId = null,
    DateTime? DueDate = null
) : IRequest<TaskDto?>;
