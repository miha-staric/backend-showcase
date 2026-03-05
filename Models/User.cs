public class User
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public String FirstName { get; set; } = "";
    public String LastName { get; set; } = "";
    public String Email { get; set; } = "";
    public List<TaskItem> Tasks { get; set; } = new();
}
