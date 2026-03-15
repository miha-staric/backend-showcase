using MediatR;

public record GetTasksQuery(Guid TenantId) : IRequest<IEnumerable<TaskDto?>>;
