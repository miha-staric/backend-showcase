using Notifications;

public interface ILoggingService
{
    Task LogUserCreatedAsync(UserCreatedNotification notification);
}
