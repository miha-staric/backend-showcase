using MediatR;
using Notifications;

public class UserCreatedLoggingHandler : INotificationHandler<UserCreatedNotification>
{
    private readonly ILoggingService _loggingService;

    public UserCreatedLoggingHandler(ILoggingService loggingService) =>
        _loggingService = loggingService;

    public Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken) =>
        _loggingService.LogUserCreatedAsync(notification);
}
