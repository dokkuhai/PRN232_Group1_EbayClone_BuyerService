
using MimeKit;
using MailKit.Net.Smtp;

namespace EbayCloneBuyerService_CoreAPI.Services.Impl
{
    public class EmailService : Services.Interface.IEmailService
    {
        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var email = Environment.GetEnvironmentVariable("SMTP_EMAIL");
            var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
            var host = Environment.GetEnvironmentVariable("SMTP_HOST");
            var port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("My App", email));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, false);
            await client.AuthenticateAsync(email, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
