using MediatR;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, IEnumerable<TaskDto?>>
{
    private readonly AppDbContext _db;
    private readonly IFusionCache _cache;
    private readonly ITenantContext _tenantContext;


    public GetTasksQueryHandler(
        AppDbContext dbContext,
        IFusionCache cache,
        ITenantContext tenantContext)
    {
        _db = dbContext;
        _cache = cache;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<TaskDto?>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        Guid? tenantId = _tenantContext.TenantId;

        string cacheKey = $"tenant:{tenantId}:tasks";

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                IEnumerable<TaskDto> taskDtos = await _db.Tasks
                .Where(t => t.TenantId == tenantId)
                .Include(t => t.AssignedUser)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    TenantId = t.TenantId,
                    Title = t.Title,
                    Status = t.Status,
                    DueDate = t.DueDate,
                    AssignedUserId = t.AssignedUserId,
                    AssignedUser = t.AssignedUser == null ? null : new UserDto
                    {
                        Id = t.AssignedUser.Id,
                        Username = t.AssignedUser.Username,
                        Email = t.AssignedUser.Email
                    }
                })
                .ToListAsync();
                return taskDtos;
            },
            new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromMinutes(5),
                IsFailSafeEnabled = true
            });
    }
}

