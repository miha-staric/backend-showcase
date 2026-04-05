using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;
using TaskManagementApi.Data;
using TaskManagementApi.Dtos.Comment;
using TaskManagementApi.Models;
using TaskManagementApi.Services.Tenancy;

namespace TaskManagementApi.Application.Comments.Commands;

public class UpdateCommentCommandHandler(
    AppDbContext db,
    IPublishEndpoint publishEndpoint,
    ITenantContext tenantContext,
    CommentCacheHelper commentCacheHelper
) : IRequestHandler<UpdateCommentCommand, CommentDto?>
{
    private readonly AppDbContext _dbContext = db;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly CommentCacheHelper _commentCacheHelper = commentCacheHelper;

    public async Task<CommentDto?> Handle(
        UpdateCommentCommand request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to update comments.");

        string cacheKey = CommentCacheHelper.GetCommentsKey(tenantId);

        Comment? comment = await _dbContext.Comments.FirstOrDefaultAsync(
            c => c.Id == request.Id && c.TenantId == request.TenantId,
            cancellationToken: cancellationToken
        );

        if (comment == null)
            return null;

        comment.TaskId = request.TaskId;
        comment.UserId = request.UserId;
        comment.Subject = request.Subject;
        comment.Content = request.Content;
        comment.UpdatedAt = DateTimeOffset.UtcNow;

        await _commentCacheHelper.InvalidateCommentCacheAsync(tenantId, comment.Id);
        _ = await _dbContext.SaveChangesAsync(cancellationToken);
        await _publishEndpoint.Publish(new CommentUpdatedEvent(comment.Id), cancellationToken);

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
