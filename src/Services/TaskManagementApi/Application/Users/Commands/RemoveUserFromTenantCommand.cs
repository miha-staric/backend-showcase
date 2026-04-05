using MediatR;

namespace TaskManagementApi.Application.Users.Commands;

public record RemoveUserFromTenantCommand(Guid UserId) : IRequest<bool>;
