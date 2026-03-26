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

    public DeleteTaskCommandHandler(
        AppDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        ITenantContext tenantContext,
        IFusionCache cache,
        TaskCacheHelper taskCacheHelper
    )
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _tenantContext = tenantContext;
        _cache = cache;
        _taskCacheHelper = taskCacheHelper;
    }

    public async Task<Boolean> Handle(
        DeleteTaskCommand request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to delete a task.");

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
