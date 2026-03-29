public class FakeEmailService : IEmailService
{
    public class SentEmail
    {
        public String Email { get; set; } = "";
        public String EmailType { get; set; } = "";
        public String Subject { get; set; } = "";
        public String Body { get; set; } = "";
        public DateTime SentAt { get; set; }
    }

    public List<SentEmail> SentEmails { get; } = new();

    public Task SendWelcomeEmail(String email)
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
        Console.WriteLine("Sending welcome email.");
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmail(String email, String resetToken)
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

    public Task SendNotificationEmail(String email, String subject, String body)
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

    public bool WasEmailSent(string email) => SentEmails.Any(e => e.Email == email);

    public SentEmail? GetLastEmail() => SentEmails.LastOrDefault();

    public void Clear() => SentEmails.Clear();
}
