using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Dtos.Comment;
using TaskManagementApi.Services.Caching;
using TaskManagementApi.Services.Tenancy;
using ZiggyCreatures.Caching.Fusion;

namespace TaskManagementApi.Application.Comments.Queries;

public class GetCommentByIdQueryHandler(
    AppDbContext dbContext,
    ITenantContext tenantContext,
    IFusionCache cache
) : IRequestHandler<GetCommentByIdQuery, CommentDto?>
{
    private readonly AppDbContext _db = dbContext;
    private readonly IFusionCache _cache = cache;
    private readonly ITenantContext _tenantContext = tenantContext;

    public async Task<CommentDto?> Handle(
        GetCommentByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to query comments.");

        string cacheKey = CommentCacheHelper.GetSingleCommentKey(tenantId, request.CommentId);

        return await _cache.GetOrSetAsync<CommentDto?>(
            cacheKey,
            async (ctx, cancellationToken) =>
            {
                CommentDto? comment = await _db
                    .Comments.Where(c => c.Id == request.CommentId)
                    .Select(c => new CommentDto
                    {
                        Id = c.Id,
                        TenantId = c.TenantId,
                        TaskId = c.TaskId,
                        UserId = c.UserId,
                        Subject = c.Subject,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (comment != null)
                    ctx.Tags = [$"task:{comment.TaskId}"];

                return comment;
            },
            token: cancellationToken
        );
    }
}
