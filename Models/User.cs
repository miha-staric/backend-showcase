public class User
{
    public Guid Id { get; set; }
    public String Username { get; set; } = "";
    public String Email { get; set; } = "";
    public List<TaskItem> Tasks { get; set; } = new();
}
