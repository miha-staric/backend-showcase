using MediatR;
using TaskManagementApi.Dtos.Task;
using TaskStatus = Contracts.Enums.TaskStatus;

namespace TaskManagementApi.Application.Tasks.Commands;

public record UpdateTaskCommand(
    Guid Id,
    Guid TenantId,
    TaskStatus Status,
    string? Title = null,
    string? Description = null,
    Guid? AssignedUserId = null,
    DateTime? DueDate = null
) : IRequest<TaskDto?>;
