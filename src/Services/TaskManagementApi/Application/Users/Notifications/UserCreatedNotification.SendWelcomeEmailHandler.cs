using MediatR;
using TaskManagementApi.Services.Email;

namespace TaskManagementApi.Application.Users.Notifications;

public class SendWelcomeEmailHandler(IEmailService emailService)
    : INotificationHandler<UserCreatedNotification>
{
    private readonly IEmailService _emailService = emailService;

    public async Task Handle(
        UserCreatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _emailService.SendWelcomeEmail(notification.Email);
    }
}
