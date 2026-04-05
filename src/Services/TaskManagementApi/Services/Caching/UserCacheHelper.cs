using ZiggyCreatures.Caching.Fusion;

namespace TaskManagementApi.Services.Caching;

public class UserCacheHelper(IFusionCache cache)
{
    private readonly IFusionCache _cache = cache;

    /// <summary>
    /// Generates the cache key for a tenant-wide user list
    /// </summary>
    /// <param name="tenantId">ID of the tenant</param>
    public static string GetUsersKey(Guid tenantId)
    {
        return $"tenant:{tenantId}:users";
    }

    /// <summary>
    /// Generates the cache key for a single user
    /// </summary>
    /// <param name="tenantId">ID of the tenant</param>
    /// <param name="userId">ID of the user</param>
    public static string GetSingleUserKey(Guid tenantId, Guid userId)
    {
        return $"tenant:{tenantId}:user:{userId}";
    }

    /// <summary>
    /// Invalidate both tenant-wide and single-user caches
    /// </summary>
    /// <param name="tenantId">ID of the tenant</param>
    /// <param name="userId">ID of the user</param>
    public async Task InvalidateUserCacheAsync(Guid tenantId, Guid userId)
    {
        string multiKey = GetUsersKey(tenantId);
        string singleKey = GetSingleUserKey(tenantId, userId);

        await _cache.RemoveAsync(multiKey);
        await _cache.RemoveAsync(singleKey);
    }
}
