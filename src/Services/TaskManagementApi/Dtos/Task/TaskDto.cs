using TaskManagementApi.Dtos.User;
using TaskStatus = Contracts.Enums.TaskStatus;

namespace TaskManagementApi.Dtos.Task;

public class TaskDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Title { get; set; } = null!;
    public TaskStatus Status { get; set; }
    public Guid? AssignedUserId { get; set; }
    public UserDto? AssignedUser { get; set; }
    public DateTimeOffset? DueDate { get; set; }
}
