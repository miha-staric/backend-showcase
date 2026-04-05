using MediatR;

namespace TaskManagementApi.Application.Users.Notifications;

public class UserCreatedNotification(Guid userId, string email) : INotification
{
    public Guid UserId { get; } = userId;
    public string Email { get; } = email;
}
