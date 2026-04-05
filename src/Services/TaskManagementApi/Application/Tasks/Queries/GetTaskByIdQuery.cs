using MediatR;
using TaskManagementApi.Dtos.Task;

namespace TaskManagementApi.Application.Tasks.Queries;

public record GetTaskByIdQuery(Guid TaskId) : IRequest<TaskDto?>;
