public interface IEmailService
{
    Task SendWelcomeEmail(String email);
    Task SendPasswordResetEmail(String email, String resetToken);
    Task SendNotificationEmail(String email, String subject, String body);
}
