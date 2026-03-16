using MediatR;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, IEnumerable<TaskDto?>>
{
    private readonly AppDbContext _db;
    private readonly IFusionCache _cache;

    public GetTasksQueryHandler(AppDbContext dbContext, IFusionCache cache)
    {
        _db = dbContext;
        _cache = cache;
    }

    public async Task<IEnumerable<TaskDto?>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"tenant:{request.TenantId}";

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                IEnumerable<TaskDto> taskDtos = await _db.Tasks
                .Where(t => t.TenantId == request.TenantId)
                .Include(t => t.AssignedUser)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    TenantId = t.TenantId,
                    Title = t.Title,
                    Status = t.Status,
                    DueDate = t.DueDate,
                    AssignedUserId = t.AssignedUserId,
                    AssignedUser = new UserDto
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

