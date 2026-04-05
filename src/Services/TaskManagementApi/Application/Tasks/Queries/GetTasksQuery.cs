using MediatR;
using TaskManagementApi.Dtos.Task;

namespace TaskManagementApi.Application.Tasks.Queries;

public record GetTasksQuery() : IRequest<IEnumerable<TaskDto?>>;
