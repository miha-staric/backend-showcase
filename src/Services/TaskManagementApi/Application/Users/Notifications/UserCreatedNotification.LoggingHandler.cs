using MediatR;
using TaskManagementApi.Services.Logging;

namespace TaskManagementApi.Application.Users.Notifications;

public class UserCreatedLoggingHandler(ILoggingService loggingService)
    : INotificationHandler<UserCreatedNotification>
{
    private readonly ILoggingService _loggingService = loggingService;

    public Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        return _loggingService.LogUserCreatedAsync(notification);
    }
}
