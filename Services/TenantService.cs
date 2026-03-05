using System.Security.Claims;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantService> _logger;

    public TenantService(IHttpContextAccessor httpContextAccessor, ILogger<TenantService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Guid GetTenantId()
    {
        ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;

        if (user == null)
        {
            _logger.LogWarning("No user context found");
            throw new InvalidOperationException("User context not found");
        }

        var tenantIdClaim = user.FindFirst("tenant_id");

        if (tenantIdClaim == null)
        {
            _logger.LogWarning("Tenant ID claim not found for user {UserId}", user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            throw new InvalidOperationException("Tenant ID not found in token");
        }

        if (Guid.TryParse(tenantIdClaim.Value, out var tenantId))
        {
            _logger.LogInformation("Retrieved tenant ID: {TenantId}", tenantId);
            return tenantId;
        }

        throw new InvalidOperationException($"Invalid tenant ID format: {tenantIdClaim.Value}");
    }
}

