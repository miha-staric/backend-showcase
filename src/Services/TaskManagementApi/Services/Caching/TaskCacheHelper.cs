using ZiggyCreatures.Caching.Fusion;

namespace TaskManagementApi.Services.Caching;

public class TaskCacheHelper(IFusionCache cache)
{
    private readonly IFusionCache _cache = cache;

    /// <summary>
    /// Generates the cache key for a tenant-wide task list
    /// </summary>
    /// <param name="tenantId">ID of the tenant</param>
    public static string GetTasksKey(Guid tenantId)
    {
        return $"tenant:{tenantId}:tasks";
    }

    /// <summary>
    /// Generates the cache key for a single task item
    /// </summary>
    /// <param name="tenantId">ID of the tenant</param>
    /// <param name="taskId">ID of the task item</param>
    public static string GetSingleTaskKey(Guid tenantId, Guid taskId)
    {
        return $"tenant:{tenantId}:task:{taskId}";
    }

    /// <summary>
    /// Invalidate both tenant-wide and single-task caches
    /// </summary>
    /// <param name="tenantId">ID of the tenant</param>
    /// <param name="taskId">ID of the task item</param>
    public async Task InvalidateTaskCacheAsync(Guid tenantId, Guid taskId)
    {
        string multiKey = GetTasksKey(tenantId);
        string singleKey = GetSingleTaskKey(tenantId, taskId);

        await _cache.RemoveAsync(multiKey);
        await _cache.RemoveAsync(singleKey);
    }
}
