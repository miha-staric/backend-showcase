using Contracts.Enums;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;
using TaskManagementApi.Data;
using TaskManagementApi.Models;
using TaskManagementApi.Services.Tenancy;

namespace TaskManagementApi.Application.Comments.Commands;

public class DeleteCommentCommandHandler(
    AppDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    ITenantContext tenantContext,
    CommentCacheHelper commentCacheHelper
) : IRequestHandler<DeleteCommentCommand, bool>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly CommentCacheHelper _commentCacheHelper = commentCacheHelper;

    public async Task<bool> Handle(
        DeleteCommentCommand request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to delete comments.");

        if (_tenantContext.UserRole != UserRole.Admin)
        {
            throw new InvalidOperationException(
                "User must have the role of Admin to delete comments."
            );
        }

        Comment? comment = await _dbContext.Comments.FirstOrDefaultAsync(
            c => c.Id == request.CommentId && c.TenantId == tenantId,
            cancellationToken
        );

        if (comment == null)
            return false;

        _ = _dbContext.Comments.Remove(comment);

        await _commentCacheHelper.InvalidateCommentCacheAsync(tenantId, request.CommentId);

        _ = await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(
            new CommentDeletedEvent(request.CommentId),
            cancellationToken
        );

        return true;
    }
}
