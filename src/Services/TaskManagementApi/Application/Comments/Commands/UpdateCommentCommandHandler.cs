using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;

public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, CommentDto?>
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ITenantContext _tenantContext;
    private readonly CommentCacheHelper _commentCacheHelper;

    public UpdateCommentCommandHandler(
        AppDbContext db,
        IPublishEndpoint publishEndpoint,
        ITenantContext tenantContext,
        CommentCacheHelper commentCacheHelper
    )
    {
        _dbContext = db;
        _publishEndpoint = publishEndpoint;
        _tenantContext = tenantContext;
        _commentCacheHelper = commentCacheHelper;
    }

    public async Task<CommentDto?> Handle(
        UpdateCommentCommand request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to update comments.");

        String cacheKey = _commentCacheHelper.GetCommentsKey(tenantId);

        Comment? comment = await _dbContext.Comments.FirstOrDefaultAsync(c =>
            c.Id == request.Id && c.TenantId == request.TenantId
        );

        if (comment == null)
            return null;

        comment.TaskId = request.TaskId;
        comment.UserId = request.UserId;
        comment.Subject = request.Subject;
        comment.Content = request.Content;
        comment.UpdatedAt = DateTimeOffset.UtcNow;

        await _commentCacheHelper.InvalidateCommentCacheAsync(tenantId, comment.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _publishEndpoint.Publish(new CommentUpdatedEvent(comment.Id));

        return new CommentDto
        {
            Id = comment.Id,
            TenantId = comment.TenantId,
            TaskId = comment.TaskId,
            UserId = comment.UserId,
            Subject = comment.Subject,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
        };
    }
}
