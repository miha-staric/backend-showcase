using Contracts.Enums;

namespace TaskManagementApi.Models;

public class UserTenant
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Username { get; set; } = "";

    public Guid TenantId { get; set; }
    public UserRole UserRole { get; set; }
}
