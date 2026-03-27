using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;
using ZiggyCreatures.Caching.Fusion;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Boolean>
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ITenantContext _tenantContext;
    private readonly IFusionCache _cache;
    private readonly TaskCacheHelper _taskCacheHelper;
    private readonly CommentCacheHelper _commentCacheHelper;

    public DeleteTaskCommandHandler(
        AppDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        ITenantContext tenantContext,
        IFusionCache cache,
        TaskCacheHelper taskCacheHelper,
        CommentCacheHelper commentCacheHelper
    )
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _tenantContext = tenantContext;
        _cache = cache;
        _taskCacheHelper = taskCacheHelper;
        _commentCacheHelper = commentCacheHelper;
    }

    public async Task<Boolean> Handle(
        DeleteTaskCommand request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to delete tasks.");

        if (_tenantContext.UserRole != UserRole.Admin)
            throw new InvalidOperationException(
                "User must have the role of Admin to delete tasks."
            );

        TaskItem? taskItem = await _dbContext.Tasks.FirstOrDefaultAsync(
            t => t.Id == request.TaskId && t.TenantId == tenantId,
            cancellationToken
        );

        if (taskItem == null)
            return false;

        _dbContext.Tasks.Remove(taskItem);

        await _taskCacheHelper.InvalidateTaskCacheAsync(tenantId, request.TaskId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new TaskDeletedEvent(request.TaskId));

        return true;
    }
}
