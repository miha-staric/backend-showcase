using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using TaskManagementApi.Data;
using TaskManagementApi.Models;

namespace TaskManagementApi.Services.Tenancy;

public class TenantMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        AppDbContext db
    )
    {
        Endpoint? endpoint = context.GetEndpoint();

        // Skip auth check if AllowAnonymous is applied
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-Tenant-Id", out StringValues tenantHeader))
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

        string? userIdString = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdString == null)
        {
            context.Response.StatusCode = 401;
            return;
        }

        Guid userId = Guid.Parse(userIdString);

        UserTenant? userTenant = await db.UserTenant.FirstOrDefaultAsync(ut =>
            ut.UserId == userId && ut.TenantId == tenantId
        );

        if (userTenant == null)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("User not part of tenant");
            return;
        }

        tenantContext.SetTenantId(tenantId);
        tenantContext.SetUserId(userId);
        tenantContext.SetUserRole(userTenant.UserRole);

        await _next(context);
    }
}
