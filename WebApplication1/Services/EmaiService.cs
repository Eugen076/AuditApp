using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string message)
        {
            Console.WriteLine($"📨 Trimitere email către: {recipientEmail}");

            var emailSettings = _configuration.GetSection("EmailSettings");

            Console.WriteLine($"📧 SMTP Server: {emailSettings["SmtpServer"]}");
            Console.WriteLine($"📧 Port: {emailSettings["Port"]}");
            Console.WriteLine($"📧 Sender Email: {emailSettings["SenderEmail"]}");
            Console.WriteLine($"📧 Password: {emailSettings["Password"]}");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"]));
            email.To.Add(new MailboxAddress("", recipientEmail));
            email.Subject = subject;

            email.Body = new TextPart("html")
            {
                Text = message
            };

            using var smtp = new SmtpClient();
            try
            {
                Console.WriteLine("🔄 Conectare la SMTP...");
                await smtp.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]), SecureSocketOptions.StartTls);
                Console.WriteLine("✅ Conectat la SMTP!");

                await smtp.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["Password"]);
                Console.WriteLine("✅ Autentificare reușită!");

                await smtp.SendAsync(email);
                Console.WriteLine("📨 Email trimis cu succes!");

                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Eroare la trimiterea email-ului: {ex.Message}");
            }
        }
    }
}
