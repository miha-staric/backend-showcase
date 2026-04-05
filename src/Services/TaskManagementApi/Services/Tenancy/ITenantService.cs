namespace TaskManagementApi.Services.Tenancy;

public interface ITenantService
{
    Task<bool> TenantExistsAsync(Guid tenantId);
}
