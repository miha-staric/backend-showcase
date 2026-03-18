public class TaskItem
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    public String Title { get; set; } = "";
    public TaskStatus Status { get; set; }

    public Guid? PrimaryAssigneeId { get; set; }
    public User? PrimaryAssigneeUser { get; set; }

    public DateTimeOffset? DueDate { get; set; }

    public List<UserTask> UserTasks { get; set; } = null!;
}

public enum TaskStatus
{
    New,
    InProgress,
    Finished,
    Closed
}
