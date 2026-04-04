namespace TaskManagementApi.Models;

public class Comment
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }

    public string Subject { get; set; } = "";
    public string Content { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public TaskItem Task { get; set; } = null!;
    public User User { get; set; } = null!;
}
