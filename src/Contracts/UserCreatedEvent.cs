namespace Contracts;

public class UserCreatedEvent(Guid userId, string email)
{
    public Guid UserId { get; } = userId;
    public string Email { get; } = email;
}
