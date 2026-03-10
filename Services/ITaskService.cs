public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetAllTasksAsync(Guid? tenantId);
    Task<TaskDto?> GetTaskByIdAsync(Guid taskId, Guid? tenantId);
}
