using MediatR;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IEnumerable<UserDto?>>
{
    private readonly AppDbContext _db;
    private readonly IFusionCache _cache;
    private readonly ITenantContext _tenantContext;


    public GetUsersQueryHandler(
        AppDbContext dbContext,
        IFusionCache cache,
        ITenantContext tenantContext)
    {
        _db = dbContext;
        _cache = cache;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<UserDto?>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        Guid? tenantId = _tenantContext.TenantId;

        string cacheKey = $"tenant:{tenantId}:users";

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                IEnumerable<UserDto> userDtos = await _db.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email
                })
                .ToListAsync();
                return userDtos;
            },
            new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromMinutes(5),
                IsFailSafeEnabled = true
            });
    }
}

