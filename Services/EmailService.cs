using System.Net;
using System.Net.Mail;

namespace PharmacyWmsBackend.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config) => _config = config;

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpHost = _config["Email:SmtpHost"] ?? "";
        if (string.IsNullOrEmpty(smtpHost))
        {
            // SMTP not configured — log instead
            Console.WriteLine($"[EMAIL] To: {to}, Subject: {subject}, Body: {body}");
            return;
        }

        using var client = new SmtpClient(smtpHost,
            int.Parse(_config["Email:SmtpPort"] ?? "587"))
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(
                _config["Email:SmtpUser"],
                _config["Email:SmtpPass"])
        };

        var from = _config["Email:From"] ?? "noreply@pharmacy-wms.local";
        var mail = new MailMessage(from, to, subject, body);
        await client.SendMailAsync(mail);
    }
}
