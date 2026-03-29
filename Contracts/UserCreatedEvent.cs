namespace Contracts;

public class UserCreatedEvent
{
    public Guid UserId { get; }
    public String Email { get; }

    public UserCreatedEvent(Guid userId, String email)
    {
        UserId = userId;
        Email = email;
    }
}
