using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using EmailSenderAPI.Models;
using Microsoft.Extensions.Logging;

namespace EmailSenderAPI.Services.Email
{
    public class EmailService(IConfiguration config, ILogger<EmailService> logger) : IEmailService
    {
        private readonly IConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));
        private readonly ILogger<EmailService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Sends both emails: one to the business and one to the customer
        public void SendEmail(EmailDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                // Send confirmation email to the user
                var confirmationEmail = CreateConfirmationEmail(request);
                SendEmailMessage(confirmationEmail);

                // Send original message to the business
                var businessEmail = CreateBusinessEmail(request);
                SendEmailMessage(businessEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Recipient}", request.To);
                throw;
            }
        }

        // Email to the customer confirming receipt
        private MimeMessage CreateConfirmationEmail(EmailDto request)
        {
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(_config["EmailUsername"]));
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = "Thank you for your message";

            email.Body = new TextPart(TextFormat.Html)
            {
                Text = "<p>Thank you for your email. We will get in touch with you shortly.</p>"
            };

            return email;
        }

        // Email to the business with the user's message
        private MimeMessage CreateBusinessEmail(EmailDto request)
        {
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(_config["EmailUsername"]));
            email.To.Add(MailboxAddress.Parse(_config["EmailUsername"]));
            email.Subject = $"New message from {request.ContactName}";

            email.Body = new TextPart(TextFormat.Html)
            {
                Text = $@"
                    <h2>New contact form submission</h2>
                    <p><strong>From:</strong> {request.ContactName} ({request.To})</p>
                    <p><strong>Message:</strong></p>
                    <p>{request.Body}</p>"
            };

            return email;
        }

        // Connects and sends the email
        private void SendEmailMessage(MimeMessage email)
        {
            using var smtp = new SmtpClient();

            var host = _config["EmailHost"];
            var portString = _config["Port"];
            var username = _config["EmailUsername"];
            var password = _config["EmailPassword"];

            if (string.IsNullOrWhiteSpace(host))
                throw new InvalidOperationException("EmailHost configuration is missing");
            if (!int.TryParse(portString, out var port) || port <= 0)
                throw new InvalidOperationException("Invalid Port configuration");
            if (string.IsNullOrWhiteSpace(username))
                throw new InvalidOperationException("EmailUsername configuration is missing");
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("EmailPassword configuration is missing");

            smtp.Connect(host, port, SecureSocketOptions.StartTls);
            smtp.Authenticate(username, password);

            try
            {
                smtp.Send(email);
            }
            finally
            {
                smtp.Disconnect(true);
            }
        }
    }
}
