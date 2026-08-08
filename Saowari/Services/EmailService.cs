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
            var originalToEmail = toEmail;

            if (interceptorActive && !string.IsNullOrEmpty(overrideEmail))
            {
                toEmail = overrideEmail;
                htmlBody = $"<p style='background-color: yellow; padding: 10px;'><b>ADMIN INTERCEPT:</b> This email was originally intended for {originalToEmail}</p>" + htmlBody;
                if (!string.IsNullOrEmpty(textBody)) {
                    textBody = $"[ADMIN INTERCEPT: Originally for {originalToEmail}]\n\n" + textBody;
                }
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Saowari Support", username));
            toEmail = toEmail?.Trim() ?? "";
            message.To.Add(new MailboxAddress("", toEmail));

            Console.WriteLine($"[DEBUG SMTP] originalToEmail: '{originalToEmail}', cleanOriginalEmail: '{originalToEmail?.Trim().ToLower()}'");

            // Forward to AdminCopyEmail if configured
            var cleanOriginalEmail = originalToEmail?.Trim().ToLower() ?? "";
            var user = _context.Users.FirstOrDefault(u => u.Email.Trim().ToLower() == cleanOriginalEmail);
            if (user != null)
            {
                Console.WriteLine($"[DEBUG SMTP] Found user ID={user.UserID}, Name={user.FullName}, Email='{user.Email}', AdminCopyEmail='{user.AdminCopyEmail}'");
                if (!string.IsNullOrEmpty(user.AdminCopyEmail))
                {
                    var cleanBcc = user.AdminCopyEmail.Trim();
                    if (!string.IsNullOrEmpty(cleanBcc))
                    {
                        Console.WriteLine($"[DEBUG SMTP] Adding BCC: '{cleanBcc}'");
                        message.Bcc.Add(new MailboxAddress("", cleanBcc));
                    }
                }
            }
            else
            {
                Console.WriteLine($"[DEBUG SMTP] User NOT found for email: '{cleanOriginalEmail}'");
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

            try
            {
                using var client = new SmtpClient();
                
                // Gmail requires STARTTLS on port 587
                var secureSocketOptions = enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
                await client.ConnectAsync(host, port, secureSocketOptions);
                
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                Console.WriteLine("[DEBUG SMTP] Email sent successfully to toEmail and Bcc!");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"[DEBUG SMTP] Exception occurred during email send: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}
