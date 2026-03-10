using Microsoft.EntityFrameworkCore;

public class TaskService : ITaskService
{
    private readonly AppDbContext _db;

    public TaskService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync(Guid? tenantId)
    {
        IEnumerable<TaskDto> taskDtos = await _db.Tasks
            .Include(t => t.AssignedUser)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Status = t.Status,
                AssignedUser = new UserDto
                {
                    Id = t.AssignedUser.Id,
                    FirstName = t.AssignedUser.FirstName,
                    LastName = t.AssignedUser.LastName
                },
                DueDate = t.DueDate
            })
            .Where(t => t.TenantId == tenantId)
            .ToListAsync();
        return taskDtos;
    }

    public async Task<TaskDto?> GetTaskByIdAsync(Guid taskId, Guid? tenantId)
    {
        TaskDto? taskDto = await _db.Tasks
          .Include(t => t.AssignedUser)
          .Select(t => new TaskDto
          {
              Id = t.Id,
              Title = t.Title,
              Status = t.Status,
              AssignedUser = new UserDto
              {
                  Id = t.AssignedUser.Id,
                  FirstName = t.AssignedUser.FirstName,
                  LastName = t.AssignedUser.LastName
              },
              DueDate = t.DueDate
          })
          .FirstOrDefaultAsync(t => t.Id == taskId && t.TenantId == tenantId);

        return taskDto;
    }
}
