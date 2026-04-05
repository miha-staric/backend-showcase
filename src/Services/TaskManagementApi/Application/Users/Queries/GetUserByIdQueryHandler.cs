using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Dtos.User;
using TaskManagementApi.Services.Caching;
using TaskManagementApi.Services.Tenancy;
using ZiggyCreatures.Caching.Fusion;

namespace TaskManagementApi.Application.Users.Queries;

public class GetUserByIdQueryHandler(
    AppDbContext dbContext,
    ITenantContext tenantContext,
    IFusionCache cache
) : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly AppDbContext _db = dbContext;
    private readonly IFusionCache _cache = cache;
    private readonly ITenantContext _tenantContext = tenantContext;

    public async Task<UserDto?> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to query users.");

        string cacheKey = UserCacheHelper.GetSingleUserKey(tenantId, request.UserId);

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                return await _db
                    .Users.Where(u => u.Id == request.UserId)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                    })
                    .FirstOrDefaultAsync(cancellationToken);
            },
            token: cancellationToken
        );
    }
}
