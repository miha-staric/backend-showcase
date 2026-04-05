using ZiggyCreatures.Caching.Fusion;

namespace TaskManagementApi.Services.Caching;

public class CommentCacheHelper(IFusionCache cache)
{
    private readonly IFusionCache _cache = cache;

    /// <summary>
    /// Generates the cache key for a tenant-wide comment list
    /// </summary>
    /// <param name="tenantId">ID of the tenant</param>
    public static string GetCommentsKey(Guid tenantId)
    {
        return $"tenant:{tenantId}:comments";
    }

    /// <summary>
    /// Generates the cache key for a single comment
    /// </summary>
    /// <param name="tenantId">ID of the tenant</param>
    /// <param name="commentId">ID of the comment</param>
    public static string GetSingleCommentKey(Guid tenantId, Guid commentId)
    {
        return $"tenant:{tenantId}:comment:{commentId}";
    }

    /// <summary>
    /// Invalidate both tenant-wide and single-comment caches
    /// </summary>
    /// <param name="tenantId">ID of the tenant</param>
    /// <param name="commentId">ID of the comment</param>
    public async Task InvalidateCommentCacheAsync(Guid tenantId, Guid commentId)
    {
        string multiKey = GetCommentsKey(tenantId);
        string singleKey = GetSingleCommentKey(tenantId, commentId);

        await _cache.RemoveAsync(multiKey);
        await _cache.RemoveAsync(singleKey);
    }
}
