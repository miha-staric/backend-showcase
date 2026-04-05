using MediatR;
using TaskManagementApi.Dtos.Task;

namespace TaskManagementApi.Application.Tasks.Commands;

public record CreateTaskCommand(
    string Title,
    string Description,
    Guid? PrimaryAssigneeId,
    DateTime? DueDate
) : IRequest<TaskDto>;
