using MediatR;

public record DeleteTaskCommand(Guid TaskId) : IRequest<Boolean>;
