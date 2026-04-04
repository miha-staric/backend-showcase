using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;
using TaskManagementApi.Dtos;
using TaskManagementApi.Models;
using TaskStatus = Contracts.Enums.TaskStatus;

public class UpdateTaskCommandHandler(
    AppDbContext db,
    IPublishEndpoint publishEndpoint,
    ITenantContext tenantContext,
    TaskCacheHelper taskCacheHelper
) : IRequestHandler<UpdateTaskCommand, TaskDto?>
{
    private readonly AppDbContext _dbContext = db;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly TaskCacheHelper _taskCacheHelper = taskCacheHelper;

    public async Task<TaskDto?> Handle(
        UpdateTaskCommand request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to update tasks.");

        string cacheKey = _taskCacheHelper.GetTasksKey(tenantId);

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
