using MediatR;

namespace Notifications;

public class UserCreatedNotification : INotification
{
    public Guid UserId { get; }
    public String Email { get; }

    public UserCreatedNotification(Guid userId, String email)
    {
        UserId = userId;
        Email = email;
    }
}
