using MassTransit;
using MediatR;
using Services.Caching;
using TaskManagementApi.Data;
using TaskManagementApi.Dtos.Comment;
using TaskManagementApi.Models;
using TaskManagementApi.Services.Tenancy;

namespace TaskManagementApi.Application.Comments.Commands;

public class CreateCommentCommandHandler(
    AppDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    ITenantContext tenantContext,
    CommentCacheHelper commentCacheHelper
) : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly CommentCacheHelper _commentCacheHelper = commentCacheHelper;

    public async Task<CommentDto> Handle(
        CreateCommentCommand request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to create comments.");

        Comment comment = new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TaskId = request.TaskId,
            UserId = request.UserId,
            Subject = request.Subject,
            Content = request.Content,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _ = _dbContext.Comments.Add(comment);

        await _commentCacheHelper.InvalidateCommentCacheAsync(tenantId, comment.Id);

        _ = await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new CommentCreatedEvent(comment.Id), cancellationToken);

        return new CommentDto
        {
            Id = comment.Id,
            TenantId = comment.TenantId,
            TaskId = comment.TaskId,
            UserId = comment.UserId,
            Subject = comment.Subject,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
        };
    }
}
