using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Saowari.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody, string textBody = "");
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly Saowari.Data.SaowariDbContext _context;

        public EmailService(IConfiguration configuration, Saowari.Data.SaowariDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string textBody = "")
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var host = smtpSettings["Host"];
            var port = int.Parse(smtpSettings["Port"]!);
            var enableSsl = bool.Parse(smtpSettings["EnableSsl"]!);
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];

            // Check Global Email Override
            var interceptorActive = _context.SystemSettings.FirstOrDefault(s => s.Key == "GlobalEmailInterceptor_Active")?.Value == "true";
            var overrideEmail = _context.SystemSettings.FirstOrDefault(s => s.Key == "GlobalEmailInterceptor_Email")?.Value;

            if (interceptorActive && !string.IsNullOrEmpty(overrideEmail))
            {
                toEmail = overrideEmail;
                htmlBody = $"<p style='background-color: yellow; padding: 10px;'><b>ADMIN INTERCEPT:</b> This email was originally intended for {toEmail}</p>" + htmlBody;
                if (!string.IsNullOrEmpty(textBody)) {
                    textBody = $"[ADMIN INTERCEPT: Originally for {toEmail}]\n\n" + textBody;
                }
            }

            var message = new MimeMessage();
            // We use the exact email address to avoid spoofing suspicion
            message.From.Add(new MailboxAddress("Saowari Support", username));
            message.To.Add(new MailboxAddress("", toEmail));

            // Forward to AdminCopyEmail if configured
            var user = _context.Users.FirstOrDefault(u => u.Email == toEmail);
            if (user != null && !string.IsNullOrEmpty(user.AdminCopyEmail))
            {
                message.Bcc.Add(new MailboxAddress("", user.AdminCopyEmail));
            }

            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            
            if (!string.IsNullOrEmpty(textBody))
            {
                builder.TextBody = textBody;
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            
            // Gmail requires STARTTLS on port 587
            var secureSocketOptions = enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(host, port, secureSocketOptions);
            
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
