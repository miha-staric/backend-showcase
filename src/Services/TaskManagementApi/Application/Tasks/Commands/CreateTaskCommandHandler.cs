using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ITenantContext _tenantContext;
    private readonly TaskCacheHelper _taskCacheHelper;

    public CreateTaskCommandHandler(
        AppDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        ITenantContext tenantContext,
        TaskCacheHelper taskCacheHelper
    )
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _tenantContext = tenantContext;
        _taskCacheHelper = taskCacheHelper;
    }

    public async Task<TaskDto> Handle(
        CreateTaskCommand request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to create tasks.");

        string cacheKey = _taskCacheHelper.GetTasksKey(tenantId);

        TaskItem task = new TaskItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Title = request.Title,
            PrimaryAssigneeId = request.PrimaryAssigneeId,
            DueDate = request.DueDate,
            Status = TaskStatus.New,
        };

        _dbContext.Tasks.Add(task);

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

            UserTask userTask = new UserTask
            {
                UserId = userId,
                TaskItemId = task.Id,
                TenantId = tenantId,
                Role = Roles.Assignee,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            _dbContext.UserTask.Add(userTask);
        }

        await _taskCacheHelper.InvalidateTaskCacheAsync(tenantId, task.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new TaskCreatedEvent(task.Id));

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
