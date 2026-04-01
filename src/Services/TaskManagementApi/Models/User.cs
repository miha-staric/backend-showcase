public class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = "";
    public string Email { get; set; } = "";

    public List<UserTask> UserTasks { get; set; } = new();
    public List<UserTenant> UserTenants { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
}
