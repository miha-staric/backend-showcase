using MediatR;
using TaskManagementApi.Dtos;

public record GetTaskByIdQuery(Guid TaskId) : IRequest<TaskDto?>;
