using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;
using ZiggyCreatures.Caching.Fusion;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Boolean>
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ITenantContext _tenantContext;
    private readonly IFusionCache _cache;
    private readonly CommentCacheHelper _commentCacheHelper;

    public DeleteCommentCommandHandler(
        AppDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        ITenantContext tenantContext,
        IFusionCache cache,
        CommentCacheHelper commentCacheHelper
    )
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _tenantContext = tenantContext;
        _cache = cache;
        _commentCacheHelper = commentCacheHelper;
    }

    public async Task<Boolean> Handle(
        DeleteCommentCommand request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to delete a comment.");

        Comment? comment = await _dbContext.Comments.FirstOrDefaultAsync(
            c => c.Id == request.CommentId && c.TenantId == tenantId,
            cancellationToken
        );

        if (comment == null)
            return false;

        _dbContext.Comments.Remove(comment);

        await _commentCacheHelper.InvalidateCommentCacheAsync(tenantId, request.CommentId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new CommentDeletedEvent(request.CommentId));

        return true;
    }
}
