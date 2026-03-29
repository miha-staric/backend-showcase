public class UserTenant
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public String Username { get; set; } = "";

    public Guid TenantId { get; set; }
    public UserRole UserRole { get; set; }
}

public enum UserRole
{
    User,
    Admin,
}
