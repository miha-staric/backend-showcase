using Microsoft.EntityFrameworkCore;

public class TaskService : ITaskService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public TaskService(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
    {
        Guid? tenantId = _tenantContext.TenantId;
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
    }

    public async Task<TaskDto?> GetTaskByIdAsync(Guid taskId)
    {
        Guid? tenantId = _tenantContext.TenantId;
        TaskDto? taskDto = await _dbContext.Tasks
          .Include(t => t.AssignedUser)
          .Select(t => new TaskDto
          {
              Id = t.Id,
              Title = t.Title,
              Status = t.Status,
              AssignedUser = new UserDto
              {
                  Id = t.AssignedUser.Id,
                  Username = t.AssignedUser.Username
              },
              DueDate = t.DueDate
          })
          .FirstOrDefaultAsync(t => t.Id == taskId && t.TenantId == tenantId);

        return taskDto;
    }
}
