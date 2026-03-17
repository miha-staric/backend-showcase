using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

public class CreateTaskCommandHandler
    : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly AppDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ITenantContext _tenantContext;
    private readonly IFusionCache _cache;

    public CreateTaskCommandHandler(
        AppDbContext db,
        IPublishEndpoint publishEndpoint,
        ITenantContext tenantContext,
        IFusionCache cache)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
        _tenantContext = tenantContext;
        _cache = cache;
    }

    public async Task<TaskDto> Handle(
        CreateTaskCommand request,
        CancellationToken cancellationToken)
    {
        Guid? tenantId = _tenantContext.TenantId;
        string cacheKey = $"tenant:{tenantId}:tasks";

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId ?? throw new Exception("TenantId missing"),
            Title = request.Title,
            AssignedUserId = request.AssignedUserId,
            DueDate = request.DueDate,
            Status = TaskStatus.New
        };

        var userId = request.AssignedUserId;

        if (userId == null || userId == Guid.Empty)
        {
            task.AssignedUserId = null;
        }
        else
        {
            Boolean userExists = await _db.Users
                .AnyAsync(u => u.Id == userId);

            if (!userExists)
                throw new Exception("Assigned user does not exist.");

            Boolean userTenantExists = await _db.UserTenant
                .AnyAsync(ut => ut.UserId == userId && ut.TenantId == tenantId);

            if (!userTenantExists)
                throw new Exception("User is not part of this tenant");

            task.AssignedUserId = userId;
        }

        _db.Tasks.Add(task);

        await _cache.RemoveAsync(cacheKey);

        await _db.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new TaskCreatedEvent(task.Id));

        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            TenantId = task.TenantId,
            AssignedUserId = task.AssignedUserId,
            DueDate = task.DueDate,
            Status = task.Status
        };
    }
}
