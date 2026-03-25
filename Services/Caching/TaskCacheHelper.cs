using ZiggyCreatures.Caching.Fusion;

namespace Services.Caching
{
    public class TaskCacheHelper
    {
        private readonly IFusionCache _cache;

        public TaskCacheHelper(IFusionCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Generates the cache key for a tenant-wide task list
        /// </summary>
        public string GetTasksKey(Guid tenantId) => $"tenant:{tenantId}:tasks";

        /// <summary>
        /// Generates the cache key for a single task item
        /// </summary>
        public string GetSingleTaskKey(Guid tenantId, Guid taskId) =>
            $"tenant:{tenantId}:task:{taskId}";

        /// <summary>
        /// Invalidate both tenant-wide and single-task caches
        /// </summary>
        public async Task InvalidateUserCacheAsync(Guid tenantId, Guid taskId)
        {
            string multiKey = GetTasksKey(tenantId);
            string singleKey = GetSingleTaskKey(tenantId, taskId);

            await _cache.RemoveAsync(multiKey);
            await _cache.RemoveAsync(singleKey);
        }
    }
}
