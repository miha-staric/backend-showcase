namespace TaskManagementApi.Services.Email;

public partial class FakeEmailService(ILogger<FakeEmailService> logger) : IEmailService
{
    public class SentEmail
    {
        public string Email { get; set; } = "";
        public string EmailType { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Body { get; set; } = "";
        public DateTime SentAt { get; set; }
    }

    public List<SentEmail> SentEmails { get; } = [];

    [LoggerMessage(Level = LogLevel.Information, Message = "Sending welcome email.")]
    private partial void Log_WelcomeEmail();

    public Task SendWelcomeEmail(string email)
    {
        SentEmails.Add(
            new SentEmail
            {
                Email = email,
                EmailType = "Welcome",
                Subject = "Welcome!",
                Body = "Welcome to our service",
                SentAt = DateTime.UtcNow,
            }
        );
        Log_WelcomeEmail();
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmail(string email, string resetToken)
    {
        SentEmails.Add(
            new SentEmail
            {
                Email = email,
                EmailType = "PasswordReset",
                Subject = "Reset Your Password",
                Body = $"Reset token: {resetToken}",
                SentAt = DateTime.UtcNow,
            }
        );
        return Task.CompletedTask;
    }

    public Task SendNotificationEmail(string email, string subject, string body)
    {
        SentEmails.Add(
            new SentEmail
            {
                Email = email,
                EmailType = "Notification",
                Subject = subject,
                Body = body,
                SentAt = DateTime.UtcNow,
            }
        );
        return Task.CompletedTask;
    }

    public bool WasEmailSent(string email)
    {
        return SentEmails.Any(e => e.Email == email);
    }

    public SentEmail? GetLastEmail()
    {
        return SentEmails.LastOrDefault();
    }

    public void Clear()
    {
        SentEmails.Clear();
    }
}
