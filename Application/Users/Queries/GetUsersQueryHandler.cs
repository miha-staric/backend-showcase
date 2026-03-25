using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;
using ZiggyCreatures.Caching.Fusion;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IEnumerable<UserDto?>>
{
    private readonly AppDbContext _db;
    private readonly IFusionCache _cache;
    private readonly ITenantContext _tenantContext;
    private readonly UserCacheHelper _userCacheHelper;

    public GetUsersQueryHandler(
        AppDbContext dbContext,
        IFusionCache cache,
        ITenantContext tenantContext,
        UserCacheHelper userCacheHelper
    )
    {
        _db = dbContext;
        _cache = cache;
        _tenantContext = tenantContext;
        _userCacheHelper = userCacheHelper;
    }

    public async Task<IEnumerable<UserDto?>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to query users.");

        string cacheKey = _userCacheHelper.GetUsersKey(tenantId);

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                IEnumerable<UserDto> userDtos = await _db
                    .Users.Select(u => new UserDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                    })
                    .ToListAsync();
                return userDtos;
            },
            new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromMinutes(5),
                IsFailSafeEnabled = true,
            }
        );
    }
}
