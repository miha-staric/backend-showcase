using MediatR;
using TaskManagementApi.Dtos;

public record CreateTaskCommand(
    string Title,
    string Description,
    Guid? PrimaryAssigneeId,
    DateTime? DueDate
) : IRequest<TaskDto>;
