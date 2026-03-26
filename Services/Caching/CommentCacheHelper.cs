using ZiggyCreatures.Caching.Fusion;

namespace Services.Caching
{
    public class CommentCacheHelper
    {
        private readonly IFusionCache _cache;

        public CommentCacheHelper(IFusionCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Generates the cache key for a tenant-wide comment list
        /// </summary>
        public string GetCommentsKey(Guid tenantId) => $"tenant:{tenantId}:comments";

        /// <summary>
        /// Generates the cache key for a single comment
        /// </summary>
        public string GetSingleCommentKey(Guid tenantId, Guid commentId) =>
            $"tenant:{tenantId}:comment:{commentId}";

        /// <summary>
        /// Invalidate both tenant-wide and single-comment caches
        /// </summary>
        public async Task InvalidateCommentCacheAsync(Guid tenantId, Guid commentId)
        {
            string multiKey = GetCommentsKey(tenantId);
            string singleKey = GetSingleCommentKey(tenantId, commentId);

            await _cache.RemoveAsync(multiKey);
            await _cache.RemoveAsync(singleKey);
        }
    }
}
