using System.Net.Mail;
using System.Net;

public static class EmailHelper
{
    public static async Task SendEmailAsync(IConfiguration config, string to, string subject, string body)
    {
        var smtpClient = new SmtpClient(config["EmailSettings:SmtpServer"])
        {
            Port = int.Parse(config["EmailSettings:Port"]),
            Credentials = new NetworkCredential(
                config["EmailSettings:SenderEmail"],
                config["EmailSettings:Password"]),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(config["EmailSettings:SenderEmail"], config["EmailSettings:SenderName"]),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };

        mailMessage.To.Add(to);

        await smtpClient.SendMailAsync(mailMessage);
    }
}
