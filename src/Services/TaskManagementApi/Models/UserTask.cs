using Contracts.Enums;

namespace TaskManagementApi.Models;

public class UserTask
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;

    public Guid TenantId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public UserTenant? UserTenant { get; set; }
}
