using MediatR;

namespace TaskManagementApi.Application.Tasks.Commands;

public record DeleteTaskCommand(Guid TaskId) : IRequest<bool>;
