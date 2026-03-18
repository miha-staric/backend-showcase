using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto?>
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ITenantContext _tenantContext;
    private readonly IFusionCache _cache;

    public UpdateTaskCommandHandler(
        AppDbContext db,
        IPublishEndpoint publishEndpoint,
        ITenantContext tenantContext,
        IFusionCache cache)
    {
        _dbContext = db;
        _publishEndpoint = publishEndpoint;
        _tenantContext = tenantContext;
        _cache = cache;
    }

    public async Task<TaskDto?> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        Guid? tenantId = _tenantContext.TenantId;
        string cacheKey = $"tenant:{request.TenantId}:task:{request.TaskId}";

        TaskItem? task = await _dbContext.Tasks.FirstOrDefaultAsync(
            t => t.Id == request.TaskId
            && t.TenantId == request.TenantId);

        if (task == null)
            return null;

        if (request.Title != null)
            task.Title = request.Title;
        if (request.AssignedUserId.HasValue)
            task.PrimaryAssigneeId = request.AssignedUserId;
        if (request.DueDate.HasValue)
            task.DueDate = request.DueDate;
        task.Status = (TaskStatus)request.Status;

        await _cache.RemoveAsync(cacheKey);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _publishEndpoint.Publish(new TaskUpdatedEvent(task.Id));

        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            AssignedUserId = task.PrimaryAssigneeId,
            DueDate = task.DueDate,
            Status = task.Status
        };
    }
}
