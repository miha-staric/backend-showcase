public interface ITenantService
{
    Task<Boolean> TenantExistsAsync(Guid tenantId);
}
