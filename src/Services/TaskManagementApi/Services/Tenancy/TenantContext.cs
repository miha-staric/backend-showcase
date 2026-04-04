using Contracts.Enums;

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public UserRole? UserRole { get; private set; }

    public void SetTenantId(Guid tenantId)
    {
        TenantId = tenantId;
    }

    public void SetUserId(Guid userId)
    {
        UserId = userId;
    }

    public void SetUserRole(UserRole userRole)
    {
        UserRole = userRole;
    }
}
