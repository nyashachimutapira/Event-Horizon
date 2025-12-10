using EventManagementSystem.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace EventManagementSystem.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task SendRsvpConfirmationAsync(string userEmail, string userName, string eventTitle, string eventDate);
        Task SendEventReminderAsync(string userEmail, string userName, string eventTitle, string eventDate, string eventLocation);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILoggingService _loggingService;

        public EmailService(IOptions<EmailSettings> emailSettings, ILoggingService loggingService)
        {
            _emailSettings = emailSettings.Value;
            _loggingService = loggingService;
        }

        public virtual async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                using (var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port))
                {
                    client.EnableSsl = _emailSettings.UseSsl;
                    client.Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_emailSettings.SenderEmail!),
                        Subject = subject,
                        Body = htmlBody,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    await _loggingService.LogInfoAsync($"Email sent to {toEmail} with subject: {subject}");
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"Failed to send email to {toEmail}", ex);
            }
        }

        public async Task SendRsvpConfirmationAsync(string userEmail, string userName, string eventTitle, string eventDate)
        {
            var subject = "RSVP Confirmation";
            var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; background: #f5f5f5; }}
                        .container {{ background: white; max-width: 600px; margin: 20px auto; padding: 20px; border-radius: 5px; }}
                        .header {{ color: #1976d2; border-bottom: 2px solid #1976d2; padding-bottom: 10px; }}
                        .content {{ margin: 20px 0; line-height: 1.6; }}
                        .footer {{ color: #666; font-size: 12px; margin-top: 20px; padding-top: 10px; border-top: 1px solid #ddd; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>RSVP Confirmation</h2>
                        </div>
                        <div class='content'>
                            <p>Hi {userName},</p>
                            <p>Thank you for confirming your attendance! We're excited to see you at our event.</p>
                            <p><strong>Event Details:</strong></p>
                            <ul>
                                <li><strong>Event:</strong> {eventTitle}</li>
                                <li><strong>Date:</strong> {eventDate}</li>
                            </ul>
                            <p>If you need to cancel your attendance, please let us know as soon as possible.</p>
                        </div>
                        <div class='footer'>
                            <p>Event Horizon Team</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(userEmail, subject, htmlBody);
        }

        public async Task SendEventReminderAsync(string userEmail, string userName, string eventTitle, string eventDate, string eventLocation)
        {
            var subject = $"Reminder: {eventTitle}";
            var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; background: #f5f5f5; }}
                        .container {{ background: white; max-width: 600px; margin: 20px auto; padding: 20px; border-radius: 5px; }}
                        .header {{ color: #ff9800; border-bottom: 2px solid #ff9800; padding-bottom: 10px; }}
                        .content {{ margin: 20px 0; line-height: 1.6; }}
                        .footer {{ color: #666; font-size: 12px; margin-top: 20px; padding-top: 10px; border-top: 1px solid #ddd; }}
                        .alert {{ background: #fff3cd; padding: 10px; border-left: 4px solid #ff9800; margin: 10px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Event Reminder</h2>
                        </div>
                        <div class='content'>
                            <p>Hi {userName},</p>
                            <div class='alert'>
                                <strong>Don't forget!</strong> Your registered event is coming up soon.
                            </div>
                            <p><strong>Event Details:</strong></p>
                            <ul>
                                <li><strong>Event:</strong> {eventTitle}</li>
                                <li><strong>Date:</strong> {eventDate}</li>
                                <li><strong>Location:</strong> {eventLocation}</li>
                            </ul>
                            <p>We look forward to seeing you there!</p>
                        </div>
                        <div class='footer'>
                            <p>Event Horizon Team</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(userEmail, subject, htmlBody);
        }
    }
}
