public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService, ITenantContext tenantContext)
    {
        if (!context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdString))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Tenant header missing");
            return;
        }
        Guid tenantId = Guid.Parse(tenantIdString);

        if (!await tenantService.TenantExistsAsync(tenantId))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Invalid tenant");
            return;
        }

        tenantContext.SetTenant(tenantId);

        await _next(context);
    }
}
