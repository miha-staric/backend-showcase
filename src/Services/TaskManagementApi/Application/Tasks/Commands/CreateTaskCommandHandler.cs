using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Dtos.Task;
using TaskManagementApi.Models;
using TaskManagementApi.Services.Caching;
using TaskManagementApi.Services.Tenancy;
using TaskStatus = Contracts.Enums.TaskStatus;

namespace TaskManagementApi.Application.Tasks.Commands;

public class CreateTaskCommandHandler(
    AppDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    ITenantContext tenantContext,
    TaskCacheHelper taskCacheHelper
) : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly TaskCacheHelper _taskCacheHelper = taskCacheHelper;

    public async Task<TaskDto> Handle(
        CreateTaskCommand request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to create tasks.");

        string cacheKey = TaskCacheHelper.GetTasksKey(tenantId);

        TaskItem task = new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Title = request.Title,
            PrimaryAssigneeId = request.PrimaryAssigneeId,
            DueDate = request.DueDate,
            Status = TaskStatus.New,
        };

        _ = _dbContext.Tasks.Add(task);

        if (request.PrimaryAssigneeId != null && request.PrimaryAssigneeId != Guid.Empty)
        {
            Guid userId = request.PrimaryAssigneeId.Value;

            bool userExists = await _dbContext.Users.AnyAsync(
                u => u.Id == userId,
                cancellationToken
            );

            if (!userExists)
                throw new InvalidOperationException("Primary assignee user does not exist.");

            bool userTenantExists = await _dbContext.UserTenant.AnyAsync(
                ut => ut.UserId == userId && ut.TenantId == tenantId,
                cancellationToken
            );

            if (!userTenantExists)
                throw new InvalidOperationException("User is not part of this tenant.");

            UserTask userTask = new()
            {
                UserId = userId,
                TaskItemId = task.Id,
                TenantId = tenantId,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            _ = _dbContext.UserTask.Add(userTask);
        }

        await _taskCacheHelper.InvalidateTaskCacheAsync(tenantId, task.Id);

        _ = await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new TaskCreatedEvent(task.Id), cancellationToken);

        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            TenantId = task.TenantId,
            AssignedUserId = task.PrimaryAssigneeId,
            DueDate = task.DueDate,
            Status = task.Status,
        };
    }
}
