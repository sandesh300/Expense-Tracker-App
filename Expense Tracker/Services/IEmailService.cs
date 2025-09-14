public interface IEmailService
{
    Task SendReportEmailAsync(string toEmail, string subject, string htmlContent, byte[] pdfAttachment, string pdfFileName);
}