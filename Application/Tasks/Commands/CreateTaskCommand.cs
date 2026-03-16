using MediatR;

public record CreateTaskCommand(
    Guid TenantId,
    string Title,
    string Description,
    Guid? AssignedUserId,
    DateTime? DueDate
) : IRequest<TaskDto>;
