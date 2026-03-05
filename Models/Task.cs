public class TaskItem
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public String Title { get; set; } = "";
    public TaskStatus Status { get; set; }
    public Guid? AssignedUserId { get; set; }
    public virtual User? AssignedUser { get; set; }
    public DateTimeOffset? DueDate { get; set; }
}

public enum TaskStatus
{
    New,
    InProgress,
    Finished,
    Closed
}
