using MediatR;

namespace Contracts;

public class UserRemovedFromTenantEvent : INotification
{
    public Guid TenantId { get; }
    public Guid UserId { get; }

    public UserRemovedFromTenantEvent(Guid tenantId, Guid userId)
    {
        TenantId = tenantId;
        UserId = userId;
    }
}
