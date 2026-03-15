using MediatR;

public record GetTaskByIdQuery(Guid TenantId, Guid TaskId) : IRequest<TaskDto?>;
