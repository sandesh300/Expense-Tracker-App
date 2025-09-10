using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System.Threading.Tasks;

// NOTE: You will need a settings class for this.
// Create a new file in your project: Settings/SmtpSettings.cs
public class SmtpSettings
{
    public string Server { get; set; }
    public int Port { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    public string Password { get; set; }
}


public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;

    public SmtpEmailService(IConfiguration configuration)
    {
        // Bind the settings from appsettings.json
        _smtpSettings = configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
    }

    public async Task SendReportEmailAsync(string toEmail, string subject, string htmlContent, byte[] pdfAttachment, string pdfFileName)
    {
        // 1. Create the main email message
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        // 2. Create the body of the email (with HTML and the PDF attachment)
        var builder = new BodyBuilder();
        builder.HtmlBody = htmlContent;

        // Add the attachment if it exists
        if (pdfAttachment != null && pdfAttachment.Length > 0)
        {
            builder.Attachments.Add(pdfFileName, pdfAttachment, ContentType.Parse("application/pdf"));
        }

        email.Body = builder.ToMessageBody();

        // 3. Create the SMTP client and send the email
        using var smtp = new SmtpClient();

        // Connect to the SMTP server (e.g., smtp.gmail.com)
        await smtp.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);

        // Authenticate with your email and the App Password
        await smtp.AuthenticateAsync(_smtpSettings.SenderEmail, _smtpSettings.Password);

        // Send the email
        await smtp.SendAsync(email);

        // Disconnect
        await smtp.DisconnectAsync(true);
    }
}