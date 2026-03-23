using MediatR;

public record RemoveUserFromTenantCommand(Guid UserId) : IRequest<Boolean>;
