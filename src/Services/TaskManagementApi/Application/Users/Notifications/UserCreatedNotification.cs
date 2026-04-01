using MediatR;

namespace Notifications;

public class UserCreatedNotification : INotification
{
    public Guid UserId { get; }
    public string Email { get; }

    public UserCreatedNotification(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}
