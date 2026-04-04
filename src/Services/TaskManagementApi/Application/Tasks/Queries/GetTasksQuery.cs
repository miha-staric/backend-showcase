using MediatR;
using TaskManagementApi.Dtos;

public record GetTasksQuery() : IRequest<IEnumerable<TaskDto?>>;
