using EmailSenderAPI.Models;

namespace EmailSenderAPI.Services.EmailService
{
    public interface IEmailService
    {
        void SendEmail(EmailDto request);
    }
}
