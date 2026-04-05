namespace TaskManagementApi.Services.Email;

public interface IEmailService
{
    Task SendWelcomeEmail(string email);
    Task SendPasswordResetEmail(string email, string resetToken);
    Task SendNotificationEmail(string email, string subject, string body);
}
