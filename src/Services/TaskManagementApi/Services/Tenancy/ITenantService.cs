public interface ITenantService
{
    Task<bool> TenantExistsAsync(Guid tenantId);
}
