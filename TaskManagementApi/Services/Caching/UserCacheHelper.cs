using ZiggyCreatures.Caching.Fusion;

namespace Services.Caching
{
    public class UserCacheHelper
    {
        private readonly IFusionCache _cache;

        public UserCacheHelper(IFusionCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Generates the cache key for a tenant-wide user list
        /// </summary>
        public string GetUsersKey(Guid tenantId) =>
            $"tenant:{tenantId}:users";

        /// <summary>
        /// Generates the cache key for a single user
        /// </summary>
        public string GetSingleUserKey(Guid tenantId, Guid userId) =>
            $"tenant:{tenantId}:user:{userId}";

        /// <summary>
        /// Invalidate both tenant-wide and single-user caches
        /// </summary>
        public async Task InvalidateUserCacheAsync(Guid tenantId, Guid userId)
        {
            string multiKey = GetUsersKey(tenantId);
            string singleKey = GetSingleUserKey(tenantId, userId);

            await _cache.RemoveAsync(multiKey);
            await _cache.RemoveAsync(singleKey);
        }
    }
}
