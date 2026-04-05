using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Dtos.Comment;
using TaskManagementApi.Services.Caching;
using TaskManagementApi.Services.Tenancy;
using ZiggyCreatures.Caching.Fusion;

namespace TaskManagementApi.Application.Comments.Queries;

public class GetCommentsQueryHandler(
    AppDbContext dbContext,
    ITenantContext tenantContext,
    IFusionCache cache
) : IRequestHandler<GetCommentsQuery, IEnumerable<CommentDto?>>
{
    private readonly AppDbContext _db = dbContext;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly IFusionCache _cache = cache;

    public async Task<IEnumerable<CommentDto?>> Handle(
        GetCommentsQuery request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to query comments.");

        string cacheKey = CommentCacheHelper.GetCommentsKey(tenantId);

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                return (IEnumerable<CommentDto>)
                    await _db
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
                        .ToListAsync(cancellationToken: _);
            },
            token: cancellationToken
        );
    }
}
