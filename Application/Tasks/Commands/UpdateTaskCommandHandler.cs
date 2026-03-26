using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;
using ZiggyCreatures.Caching.Fusion;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto?>
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ITenantContext _tenantContext;
    private readonly TaskCacheHelper _taskCacheHelper;

    public UpdateTaskCommandHandler(
        AppDbContext db,
        IPublishEndpoint publishEndpoint,
        ITenantContext tenantContext,
        TaskCacheHelper taskCacheHelper
    )
    {
        _dbContext = db;
        _publishEndpoint = publishEndpoint;
        _tenantContext = tenantContext;
        _taskCacheHelper = taskCacheHelper;
    }

    public async Task<TaskDto?> Handle(
        UpdateTaskCommand request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to create tasks.");

        String cacheKey = _taskCacheHelper.GetTasksKey(tenantId);

        TaskItem? task = await _dbContext.Tasks.FirstOrDefaultAsync(t =>
            t.Id == request.Id && t.TenantId == request.TenantId
        );

        if (task == null)
            return null;

        if (request.Title != null)
            task.Title = request.Title;
        if (request.AssignedUserId.HasValue)
            task.PrimaryAssigneeId = request.AssignedUserId;
        if (request.DueDate.HasValue)
            task.DueDate = request.DueDate;
        task.Status = (TaskStatus)request.Status;

        await _taskCacheHelper.InvalidateTaskCacheAsync(tenantId, task.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _publishEndpoint.Publish(new TaskUpdatedEvent(task.Id));

        return new TaskDto
        {
            Id = task.Id,
            TenantId = task.TenantId,
            Title = task.Title,
            AssignedUserId = task.PrimaryAssigneeId,
            DueDate = task.DueDate,
            Status = task.Status,
        };
    }
}
