using Microsoft.EntityFrameworkCore;

public class TenantService : ITenantService
{
    private readonly AppDbContext _dbContext;

    public TenantService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> TenantExistsAsync(Guid tenantId)
    {
        return await _dbContext.UserTenant
            .AnyAsync(t => t.TenantId == tenantId);
    }
}
