using MediatR;

public record GetTasksQuery() : IRequest<IEnumerable<TaskDto?>>;
