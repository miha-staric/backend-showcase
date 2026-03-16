using MediatR;

public record UpdateTaskCommand(
    Guid TenantId,
    Guid TaskId,
    TaskStatus Status,
    string? Title = null,
    string? Description = null,
    Guid? AssignedUserId = null,
    DateTime? DueDate = null
) : IRequest<TaskDto?>;
