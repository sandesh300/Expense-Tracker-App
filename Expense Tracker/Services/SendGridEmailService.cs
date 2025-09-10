// In Services/SendGridEmailService.cs
using Microsoft.Extensions.Configuration; // Can remove this if not used elsewhere
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

// Your settings class might be in a different namespace

public class SendGridEmailService : IEmailService
{
    // Inject the strongly-typed settings object
    private readonly SendGridSettings _sendGridSettings;

    public SendGridEmailService(SendGridSettings sendGridSettings)
    {
        _sendGridSettings = sendGridSettings;
    }

    public async Task SendReportEmailAsync(string toEmail, string subject, string htmlContent, byte[] pdfAttachment, string pdfFileName)
    {
        // Fetch settings from the injected settings object - much cleaner!
        var apiKey = _sendGridSettings.ApiKey;
        var fromEmail = _sendGridSettings.FromEmail;
        var fromName = _sendGridSettings.FromName;

        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(fromEmail, fromName),
            Subject = subject,
            HtmlContent = htmlContent
        };
        msg.AddTo(new EmailAddress(toEmail));

        if (pdfAttachment != null && pdfAttachment.Length > 0)
        {
            var attachmentContent = Convert.ToBase64String(pdfAttachment);
            msg.AddAttachment(pdfFileName, attachmentContent, "application/pdf", "attachment");
        }

        var response = await client.SendEmailAsync(msg);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Body.ReadAsStringAsync();
            // You should log this error properly in a real application
            Console.WriteLine($"Failed to send email. Status Code: {response.StatusCode}. Error: {errorBody}");
            throw new InvalidOperationException($"Failed to send email: {response.StatusCode}");
        }
    }
}