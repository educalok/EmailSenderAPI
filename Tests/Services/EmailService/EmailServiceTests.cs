using System;
using System.Reflection;
using EmailSenderAPI.Models;
using EmailSenderAPI.Services.Email;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Moq;
using Xunit;

namespace EmailSenderAPI.Tests.Services.Email
{
    public class EmailServiceTests
    {
        private readonly Mock<IConfiguration> _configMock = new();
        private readonly Mock<ILogger<EmailService>> _loggerMock = new();

        private void SetupValidConfig()
        {
            _configMock.Setup(c => c["EmailHost"]).Returns("smtp.test.com");
            _configMock.Setup(c => c["EmailUsername"]).Returns("test@test.com");
            _configMock.Setup(c => c["EmailPassword"]).Returns("password");
            _configMock.Setup(c => c["Port"]).Returns("587");
        }

        [Fact]
        public void SendEmail_ThrowsArgumentNullException_WhenRequestIsNull()
        {
            // Arrange
            SetupValidConfig();
            var service = new EmailService(_configMock.Object, _loggerMock.Object);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => service.SendEmail(null));
        }

        [Fact]
        public async Task SendEmail_LogsErrorAndThrows_WhenExceptionOccurs()
        {
            // Arrange
            SetupValidConfig();
            var service = new EmailService(_configMock.Object, _loggerMock.Object);
            var request = new EmailDto { To = "user@test.com", ContactName = "User", Body = "Hello" };

            // Simulate missing host to cause exception
            _configMock.Setup(c => c["EmailHost"]).Returns(value: null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => Task.Run(() => service.SendEmail(request)));
            Assert.Equal("EmailHost configuration is missing", ex.Message);
        }

        [Fact]
        public void SendEmailMessage_Throws_WhenHostInvalid()
        {
            // Arrange
            SetupValidConfig();
            _ = _configMock.Setup(c => c["EmailHost"]).Returns(value: null); // Simulate missing host
            var service = new EmailService(_configMock.Object, _loggerMock.Object);

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("test@test.com"));
            email.To.Add(MailboxAddress.Parse("user@test.com"));
            email.Subject = "Test";
            email.Body = new TextPart("plain") { Text = "Test" };

            // Corrected the type reference to avoid CS0426 error
            var method = typeof(EmailService).GetMethod("SendEmailMessage", bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance);

            // Define the expected message to fix CS0103
            string expectedMessage = "EmailHost configuration is missing";

            // Act & Assert
            var ex = Assert.Throws<TargetInvocationException>(() => method.Invoke(service, new object[] { email }));
            Assert.IsType<InvalidOperationException>(ex.InnerException);
            Assert.Equal(expectedMessage, ex.InnerException.Message);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConfigIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new EmailService(null, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new EmailService(_configMock.Object, null));
        }

        [Fact]
        public void CreateConfirmationEmail_ReturnsValidMimeMessage()
        {
            // Arrange
            SetupValidConfig();
            var service = new EmailService(_configMock.Object, _loggerMock.Object);
            var request = new EmailDto { To = "user@test.com", ContactName = "User", Body = "Hello" };

            // Use reflection to call private method
            var method = typeof(EmailService).GetMethod("CreateConfirmationEmail", bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance);

            // Ensure the method is not null before invoking
            Assert.NotNull(method);

            // Act
            var result = (MimeMessage?)method!.Invoke(service, new object[] { request });

            // Assert
            Assert.NotNull(result); // Ensure result is not null
            Assert.Equal("Thank you for your message", result.Subject);
            Assert.Contains("Thank you for your email", result.TextBody ?? result.HtmlBody);
            Assert.Contains("user@test.com", result.To.ToString());
        }

        [Fact]
        public void CreateBusinessEmail_ReturnsValidMimeMessage()
        {
            // Arrange
            SetupValidConfig();
            var service = new EmailService(_configMock.Object, _loggerMock.Object);
            var request = new EmailDto { To = "user@test.com", ContactName = "User", Body = "Hello" };

            // Use reflection to call private method
            var method = typeof(EmailService).GetMethod("CreateBusinessEmail", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            MimeMessage? result = method.Invoke(service, new object[] { request }) as MimeMessage;

            // Assert
            Assert.NotNull(result); // Ensure result is not null to avoid CS8600
            Assert.Contains("New message from User", result.Subject);
            Assert.Contains("New contact form submission", result.HtmlBody);
            Assert.Contains("Hello", result.HtmlBody);
        }
    }
}