using MediatR;

namespace TaskManagementApi.Application.Users.Commands;

public class UserRemovedFromTenantEvent(Guid tenantId, Guid userId) : INotification
{
    public Guid TenantId { get; } = tenantId;
    public Guid UserId { get; } = userId;
}
