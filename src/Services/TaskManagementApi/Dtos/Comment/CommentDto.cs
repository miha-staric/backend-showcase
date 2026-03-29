public class CommentDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }

    public String Subject { get; set; } = "";
    public String Content { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
