using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ITenantContext _tenantContext;
    private readonly CommentCacheHelper _commentCacheHelper;

    public CreateCommentCommandHandler(
        AppDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        ITenantContext tenantContext,
        CommentCacheHelper commentCacheHelper
    )
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _tenantContext = tenantContext;
        _commentCacheHelper = commentCacheHelper;
    }

    public async Task<CommentDto> Handle(
        CreateCommentCommand request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to create comments.");

        string cacheKey = _commentCacheHelper.GetCommentsKey(tenantId);

        Comment comment = new Comment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TaskId = request.TaskId,
            UserId = request.UserId,
            Subject = request.Subject,
            Content = request.Content,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _dbContext.Comments.Add(comment);

        await _commentCacheHelper.InvalidateCommentCacheAsync(tenantId, comment.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new CommentCreatedEvent(comment.Id));

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
