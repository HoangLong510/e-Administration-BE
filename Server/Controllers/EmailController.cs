using Microsoft.AspNetCore.Mvc;
using Server.Repositories;
using Server.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailRepository _emailRepository;

        public EmailController(IEmailRepository emailRepository)
        {
            _emailRepository = emailRepository;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailModel emailModel)
        {
            if (emailModel == null)
            {
                return BadRequest("Email data is required");
            }

            await _emailRepository.SendEmailAsync(emailModel.ToEmail, emailModel.Subject, emailModel.Body);
            return Ok("Email sent successfully");
        }

        [HttpGet("last")]
        public async Task<IActionResult> GetLastEmail([FromQuery] string toEmail, [FromQuery] string subject)
        {
            var email = await _emailRepository.GetLastEmailAsync(toEmail, subject);

            if (email == null)
            {
                return NotFound("No email found for the given criteria");
            }

            return Ok(email);
        }

        [HttpPost("update-status")]
        public async Task<IActionResult> UpdateEmailStatus()
        {
            var updatedEmails = await _emailRepository.UpdateStatusAsync();
            return Ok(updatedEmails);
        }

        [HttpGet("instructor-emails")]
        public async Task<IActionResult> GetInstructorEmailsForLab()
        {
            var emails = await _emailRepository.GetInstructorEmailsForLabAsync();
            return Ok(emails);
        }

        [HttpPost("resend")]
        public async Task<IActionResult> ResendEmailIfNecessary([FromBody] EmailModel emailModel)
        {
            if (emailModel == null)
            {
                return BadRequest("Email data is required");
            }

            await _emailRepository.ResendEmailIfNecessaryAsync(emailModel.ToEmail, emailModel.Subject, emailModel.Body);
            return Ok("Email resent successfully if needed");
        }
    }
}
