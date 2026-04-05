using TaskManagementApi.Application.Users.Notifications;

namespace TaskManagementApi.Services.Logging;

public interface ILoggingService
{
    Task LogUserCreatedAsync(UserCreatedNotification notification);
}
