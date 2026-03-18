using MediatR;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto?>
{
    private readonly AppDbContext _db;
    private readonly IFusionCache _cache;
    private readonly ITenantContext _tenantContext;

    public GetTaskByIdQueryHandler(
        AppDbContext dbContext,
        ITenantContext tenantContext,
        IFusionCache cache)
    {
        _db = dbContext;
        _tenantContext = tenantContext;
        _cache = cache;
    }

    public async Task<TaskDto?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        Guid? tenantId = _tenantContext.TenantId;
        string cacheKey = $"tenant:{tenantId}:task:{request.TaskId}";

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                var task = await _db.Tasks
                    .Include(t => t.PrimaryAssigneeUser)
                    .Where(t => t.Id == request.TaskId && t.TenantId == tenantId)
                    .Select(t => new TaskDto
                    {
                        Id = t.Id,
                        TenantId = t.TenantId,
                        Title = t.Title,
                        Status = t.Status,
                        DueDate = t.DueDate,
                        AssignedUserId = t.PrimaryAssigneeId,
                        AssignedUser = t.PrimaryAssigneeUser != null ? new UserDto
                        {
                            Id = t.PrimaryAssigneeUser.Id,
                            Username = t.PrimaryAssigneeUser.Username,
                            Email = t.PrimaryAssigneeUser.Email
                        } : null
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return task;
            },
            new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromMinutes(5),
                IsFailSafeEnabled = true
            });
    }
}
