using TaskManagementApi.Application.Users.Notifications;

namespace TaskManagementApi.Services.Logging;

public partial class LoggingService(ILogger<LoggingService> logger) : ILoggingService
{
    [LoggerMessage(Level = LogLevel.Information, Message = "User created.")]
    private partial void Log_UserCreated();

    public Task LogUserCreatedAsync(UserCreatedNotification notification)
    {
        Log_UserCreated();
        return Task.CompletedTask;
    }
}
