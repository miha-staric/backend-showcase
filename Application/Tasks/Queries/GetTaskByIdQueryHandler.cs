using MediatR;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto?>
{
    private readonly AppDbContext _db;
    private readonly IFusionCache _cache;

    public GetTaskByIdQueryHandler(AppDbContext db, IFusionCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<TaskDto?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"tenant:{request.TenantId}:task:{request.TaskId}";

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                var task = await _db.Tasks
                    .Include(t => t.AssignedUser)
                    .Where(t => t.Id == request.TaskId && t.TenantId == request.TenantId)
                    .Select(t => new TaskDto
                    {
                        Id = t.Id,
                        Status = t.Status,
                        AssignedUser = t.AssignedUser != null ? new UserDto
                        {
                            Id = t.AssignedUser.Id,
                            Username = t.AssignedUser.Username
                        } : null,
                        DueDate = t.DueDate
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
