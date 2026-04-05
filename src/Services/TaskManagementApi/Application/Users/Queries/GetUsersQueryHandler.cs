using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Dtos.User;
using TaskManagementApi.Services.Caching;
using TaskManagementApi.Services.Tenancy;
using ZiggyCreatures.Caching.Fusion;

namespace TaskManagementApi.Application.Users.Queries;

public class GetUsersQueryHandler(
    AppDbContext dbContext,
    IFusionCache cache,
    ITenantContext tenantContext
) : IRequestHandler<GetUsersQuery, IEnumerable<UserDto?>>
{
    private readonly AppDbContext _db = dbContext;
    private readonly IFusionCache _cache = cache;
    private readonly ITenantContext _tenantContext = tenantContext;

    public async Task<IEnumerable<UserDto?>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to query users.");

        string cacheKey = UserCacheHelper.GetUsersKey(tenantId);

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                return (IEnumerable<UserDto>)
                    await _db
                        .Users.Select(u => new UserDto
                        {
                            Id = u.Id,
                            Username = u.Username,
                            Email = u.Email,
                        })
                        .ToListAsync(cancellationToken: _);
            },
            token: cancellationToken
        );
    }
}
