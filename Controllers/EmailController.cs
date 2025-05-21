using System.Net;
using EmailSenderAPI.Models;
using EmailSenderAPI.Services.Email;
using Microsoft.AspNetCore.Mvc;

namespace EmailSenderAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EmailController(IEmailService emailService) : ControllerBase
	{
		// Service for sending emails
		private readonly IEmailService _emailService = emailService;

		// POST api/email
		[HttpPost]
		public IActionResult SendEmail(EmailDto request)
		{
			// Call the service to send the email
			_emailService.SendEmail(request);

			// Return success response
			return Ok(new { message = "Email sent successfully" });
		}
	}
}