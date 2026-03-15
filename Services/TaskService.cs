using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

public class TaskService : ITaskService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IFusionCache _cache;

    public TaskService(
        AppDbContext dbContext,
        ITenantContext tenantContext,
        IFusionCache cache
        )
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _cache = cache;
    }

    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
    {
        Guid? tenantId = _tenantContext.TenantId;

        String cacheKey = $"tasks:{tenantId}";
        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                IEnumerable<TaskDto> taskDtos = await _dbContext.Tasks
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
          }
        );
    }

    public async Task<TaskDto?> GetTaskByIdAsync(Guid taskId)
    {
        Guid? tenantId = _tenantContext.TenantId;
        String cacheKey = $"task:{tenantId}-{taskId}";

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                TaskDto? taskDto = await _dbContext.Tasks
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
                  .FirstOrDefaultAsync(t => t.Id == taskId && t.TenantId == tenantId);

                return taskDto;
            },
          new FusionCacheEntryOptions
          {
              Duration = TimeSpan.FromMinutes(5),
              IsFailSafeEnabled = true
          }
        );
    }
}
