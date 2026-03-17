using MediatR;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly AppDbContext _db;
    private readonly IFusionCache _cache;
    private readonly ITenantContext _tenantContext;

    public GetUserByIdQueryHandler(
        AppDbContext dbContext,
        ITenantContext tenantContext,
        IFusionCache cache)
    {
        _db = dbContext;
        _tenantContext = tenantContext;
        _cache = cache;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        Guid? tenantId = _tenantContext.TenantId;
        string cacheKey = $"tenant:{tenantId}:user:{request.UserId}";

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                var user = await _db.Users
                    .Where(u => u.Id == request.UserId)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return user;
            },
            new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromMinutes(5),
                IsFailSafeEnabled = true
            });
    }
}
