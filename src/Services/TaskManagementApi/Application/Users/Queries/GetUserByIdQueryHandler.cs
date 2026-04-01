using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;
using ZiggyCreatures.Caching.Fusion;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly AppDbContext _db;
    private readonly IFusionCache _cache;
    private readonly ITenantContext _tenantContext;
    private readonly UserCacheHelper _userCacheHelper;

    public GetUserByIdQueryHandler(
        AppDbContext dbContext,
        ITenantContext tenantContext,
        IFusionCache cache,
        UserCacheHelper userCacheHelper
    )
    {
        _db = dbContext;
        _tenantContext = tenantContext;
        _cache = cache;
        _userCacheHelper = userCacheHelper;
    }

    public async Task<UserDto?> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to query users.");

        string cacheKey = _userCacheHelper.GetSingleUserKey(tenantId, request.UserId);

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                UserDto? user = await _db
                    .Users.Where(u => u.Id == request.UserId)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return user;
            }
        );
    }
}
