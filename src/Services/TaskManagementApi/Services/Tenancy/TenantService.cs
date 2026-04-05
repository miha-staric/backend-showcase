using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Services.Tenancy;

public class TenantService(AppDbContext dbContext) : ITenantService
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<bool> TenantExistsAsync(Guid tenantId)
    {
        return await _dbContext.UserTenant.AnyAsync(t => t.TenantId == tenantId);
    }
}
