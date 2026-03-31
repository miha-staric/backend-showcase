using Notifications;

public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;

    public LoggingService(ILogger<LoggingService> logger) => _logger = logger;

    public Task LogUserCreatedAsync(UserCreatedNotification notification)
    {
        _logger.LogInformation("User created: {@Notification}", notification);
        return Task.CompletedTask;
    }
}
