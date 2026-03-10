using System.Security.Claims;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantService> _logger;
    private readonly AppDbContext _dbContext;

    public TenantService(IHttpContextAccessor httpContextAccessor, AppDbContext dbContext, ILogger<TenantService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Guid?> GetTenantId()
    {
        ClaimsPrincipal? user = _httpContextAccessor.HttpContext?.User;

        if (user == null)
        {
            _logger.LogWarning("No user context found");
            throw new InvalidOperationException("User context not found");
        }

        var issuer = user.FindFirst("iss");

        if (issuer == null)
        {
            _logger.LogWarning("Issuer not found for user {UserId}", user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            throw new InvalidOperationException("Issuer not found in token");
        }

        String realm = issuer.Value.Split('/').Last();
        _logger.LogInformation("Retrieved realm: {realm}", realm);

        TenantDto? tenant = _dbContext.Tenants
          .Select(t => new TenantDto
          {
              Id = t.Id,
              Title = t.Title,
              Enabled = t.Enabled
          })
          .FirstOrDefault(t => t.Title == realm);

        if (tenant != null)
            return tenant.Id;

        throw new InvalidOperationException($"Invalid tenant ID format: {issuer.Value}");
    }
}

