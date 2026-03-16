using MediatR;

public record CreateTaskCommand(
    string Title,
    string Description,
    Guid? AssignedUserId,
    DateTime? DueDate
) : IRequest<TaskDto>;
