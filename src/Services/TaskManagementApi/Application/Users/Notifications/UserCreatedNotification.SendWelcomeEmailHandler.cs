using Contracts;
using MediatR;

public class SendWelcomeEmailHandler : INotificationHandler<UserCreatedNotification>
{
    private readonly IEmailService _emailService;

    public SendWelcomeEmailHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Handle(
        UserCreatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _emailService.SendWelcomeEmail(notification.Email);
    }
}
