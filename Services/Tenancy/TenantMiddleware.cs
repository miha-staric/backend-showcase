using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        AppDbContext db)
    {
        if (!context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Tenant header missing");
            return;
        }

        if (!Guid.TryParse(tenantHeader, out Guid tenantId))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid tenant id");
            return;
        }

        String? userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            context.Response.StatusCode = 401;
            return;
        }

        bool userInTenant = await db.UserTenant
            .AnyAsync(x =>
                x.TenantId == tenantId &&
                x.UserId == Guid.Parse(userId));

        if (!userInTenant)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("User not part of tenant");
            return;
        }

        tenantContext.SetTenant(tenantId);

        await _next(context);
    }
}
