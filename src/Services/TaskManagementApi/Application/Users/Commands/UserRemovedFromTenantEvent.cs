using MediatR;

namespace Contracts;

public class UserRemovedFromTenantEvent(Guid tenantId, Guid userId) : INotification
{
    public Guid TenantId { get; } = tenantId;
    public Guid UserId { get; } = userId;
}
