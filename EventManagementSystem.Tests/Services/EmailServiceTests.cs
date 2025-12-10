using Xunit;
using System;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Options;
using EventManagementSystem.Services;
using EventManagementSystem.Configuration;

namespace EventManagementSystem.Tests.Services
{
    public class EmailServiceTests
    {
        private readonly Mock<ILoggingService> _mockLoggingService;
        private readonly EmailSettings _emailSettings;
        private readonly Mock<EmailService> _mockEmailService;

        public EmailServiceTests()
        {
            _mockLoggingService = new Mock<ILoggingService>();
            _emailSettings = new EmailSettings
            {
                SmtpServer = "smtp.gmail.com",
                Port = 587,
                SenderEmail = "test@example.com",
                SenderPassword = "password",
                UseSsl = true
            };

            var options = Options.Create(_emailSettings);
            
            // Create a partial mock of EmailService
            _mockEmailService = new Mock<EmailService>(options, _mockLoggingService.Object) { CallBase = true };
        }

        [Fact]
        public async Task SendRsvpConfirmationAsync_Should_Call_SendEmailAsync()
        {
            // Arrange
            string userEmail = "user@example.com";
            string userName = "John Doe";
            string eventTitle = "Tech Conference";
            string eventDate = "2025-12-15";

            // Setup the mock to do nothing when SendEmailAsync is called
            _mockEmailService
                .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _mockEmailService.Object.SendRsvpConfirmationAsync(userEmail, userName, eventTitle, eventDate);

            // Assert
            _mockEmailService.Verify(
                x => x.SendEmailAsync(userEmail, "RSVP Confirmation", It.Is<string>(s => s.Contains(userName) && s.Contains(eventTitle))),
                Times.Once
            );
        }

        [Fact]
        public async Task SendEventReminderAsync_Should_Create_Valid_Email()
        {
            // Arrange
            string userEmail = "user@example.com";
            string userName = "Jane Smith";
            string eventTitle = "Web Summit";
            string eventDate = "2025-12-20";
            string eventLocation = "New York";

            // Setup the mock to do nothing when SendEmailAsync is called
            _mockEmailService
                .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _mockEmailService.Object.SendEventReminderAsync(userEmail, userName, eventTitle, eventDate, eventLocation);

            // Assert
            _mockEmailService.Verify(
                x => x.SendEmailAsync(userEmail, $"Reminder: {eventTitle}", It.Is<string>(s => s.Contains(userName) && s.Contains(eventLocation))),
                Times.Once
            );
        }

        [Fact]
        public async Task SendEmailAsync_With_Invalid_Settings_Should_Log_Error()
        {
            // This test verifies error handling when email sending fails
            // We can't easily mock SmtpClient, so we'll rely on the fact that with invalid settings/no server, it WILL throw
            // and we check if LogErrorAsync is called.
            
            // Note: In a real unit test environment without network, SmtpClient might fail fast.
            // However, since we are testing the REAL SendEmailAsync here (no mock setup), it will try to connect.
            
            // Arrange
            var service = new EmailService(Options.Create(_emailSettings), _mockLoggingService.Object);
            string toEmail = "test@invalid.com";
            string subject = "Test";
            string body = "Test body";

            // Act
            await service.SendEmailAsync(toEmail, subject, body);

            // Assert - Email service should attempt to log any errors
            _mockLoggingService.Verify(
                x => x.LogErrorAsync(It.IsAny<string>(), It.IsAny<Exception>()),
                Times.Once
            );
        }
    }
}
