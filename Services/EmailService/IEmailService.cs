using EmailSenderAPI.Models;

namespace EmailSenderAPI.Services.Email
{
    public interface IEmailService
    {
        void SendEmail(EmailDto request);
    }
}
