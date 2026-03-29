public interface ITenantContext
{
    Guid? TenantId { get; }
    Guid? UserId { get; }
    UserRole? UserRole { get; }
    void SetTenantId(Guid tenantId);
    void SetUserId(Guid userId);
    void SetUserRole(UserRole userRole);
}
