using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;
using TaskManagementApi.Data;
using TaskManagementApi.Models;
using TaskManagementApi.Services.Caching;
using TaskManagementApi.Services.Tenancy;
using ZiggyCreatures.Caching.Fusion;
using UserRole = Contracts.Enums.UserRole;

namespace TaskManagementApi.Application.Tasks.Commands;

public class DeleteTaskCommandHandler(
    AppDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    ITenantContext tenantContext,
    IFusionCache cache,
    TaskCacheHelper taskCacheHelper,
    CommentCacheHelper commentCacheHelper
) : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly IFusionCache _cache = cache;
    private readonly TaskCacheHelper _taskCacheHelper = taskCacheHelper;
    private readonly CommentCacheHelper _commentCacheHelper = commentCacheHelper;

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to delete tasks.");

        if (_tenantContext.UserRole != UserRole.Admin)
        {
            throw new InvalidOperationException(
                "User must have the role of Admin to delete tasks."
            );
        }

        TaskItem? taskItem = await _dbContext.Tasks.FirstOrDefaultAsync(
            t => t.Id == request.TaskId && t.TenantId == tenantId,
            cancellationToken
        );

        if (taskItem == null)
            return false;

        _ = _dbContext.Tasks.Remove(taskItem);

        await _taskCacheHelper.InvalidateTaskCacheAsync(tenantId, request.TaskId);
        _cache.RemoveByTag($"task:{request.TaskId}", token: cancellationToken);

        _ = await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new TaskDeletedEvent(request.TaskId), cancellationToken);

        return true;
    }
}
