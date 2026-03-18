public class User
{
    public Guid Id { get; set; }

    public String Username { get; set; } = "";
    public String Email { get; set; } = "";

    public List<UserTask> UserTasks { get; set; } = new();
    public List<UserTenant> UserTenants { get; set; } = new();
}
