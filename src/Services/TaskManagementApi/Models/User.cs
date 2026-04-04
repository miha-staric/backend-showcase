namespace TaskManagementApi.Models;

public class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = "";
    public string Email { get; set; } = "";

    public List<UserTask> UserTasks { get; set; } = [];
    public List<UserTenant> UserTenants { get; set; } = [];
    public List<Comment> Comments { get; set; } = [];
}
