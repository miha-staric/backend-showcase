using MediatR;

public record GetTaskByIdQuery(Guid TaskId) : IRequest<TaskDto?>;
