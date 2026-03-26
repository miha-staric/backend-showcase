using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;
using ZiggyCreatures.Caching.Fusion;

public class GetCommentsQueryHandler : IRequestHandler<GetCommentsQuery, IEnumerable<CommentDto?>>
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IFusionCache _cache;
    private readonly CommentCacheHelper _commentCacheHelper;

    public GetCommentsQueryHandler(
        AppDbContext dbContext,
        ITenantContext tenantContext,
        IFusionCache cache,
        CommentCacheHelper commentCacheHelper
    )
    {
        _db = dbContext;
        _cache = cache;
        _tenantContext = tenantContext;
        _commentCacheHelper = commentCacheHelper;
    }

    public async Task<IEnumerable<CommentDto?>> Handle(
        GetCommentsQuery request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to query comments.");

        string cacheKey = _commentCacheHelper.GetCommentsKey(tenantId);

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                IEnumerable<CommentDto> commentDtos = await _db
                    .Comments.Select(c => new CommentDto
                    {
                        Id = c.Id,
                        TaskId = c.TaskId,
                        TenantId = c.TenantId,
                        UserId = c.UserId,
                        Subject = c.Subject,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                    })
                    .ToListAsync();
                return commentDtos;
            }
        );
    }
}
