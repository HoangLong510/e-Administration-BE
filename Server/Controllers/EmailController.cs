using Microsoft.AspNetCore.Mvc;
using Server.Data;
using Server.Models.Enums;
using Server.Models;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class EmailController : ControllerBase
{
    private readonly EmailService _emailService;
    private readonly DatabaseContext db;

    public EmailController(EmailService emailService, DatabaseContext db)
    {
        _emailService = emailService;
        this.db = db;
    }

    [HttpPost("send")]
    public IActionResult SendEmail([FromBody] EmailModel emailModel)
    {
        if (string.IsNullOrEmpty(emailModel.ToEmail) || string.IsNullOrEmpty(emailModel.Subject) || string.IsNullOrEmpty(emailModel.Body))
        {
            return BadRequest("Invalid email data.");
        }

        try
        {
            var lastEmail = db.Emails
                .Where(e => e.ToEmail == emailModel.ToEmail && e.Subject == emailModel.Subject)
                .OrderByDescending(e => e.SentDate)
                .FirstOrDefault();

            if (lastEmail != null && lastEmail.SentDate.HasValue && (DateTime.Now - lastEmail.SentDate.Value).Days < 7)
            {
                return BadRequest("You can only send this email once per week.");
            }

            _emailService.SendEmail(emailModel.ToEmail, emailModel.Subject, emailModel.Body);

            emailModel.SentDate = DateTime.Now;
            emailModel.Status = "Sent";
            db.Emails.Add(emailModel);
            db.SaveChanges();

            return Ok("Email sent successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("expiring-software")]
    public List<Software> GetExpiringSoftware()
    {
        var currentDate = DateTime.Now;
        var expirationDate = currentDate.AddMonths(1);

        return db.Softwares
            .Where(s => s.LicenseExpire.HasValue && s.LicenseExpire.Value <= expirationDate && s.LicenseExpire.Value > currentDate)
            .ToList();
    }

    [HttpGet("emails-for-software")]
    public async Task<List<string>> GetEmailsForSoftware([FromQuery] List<int> softwareIds)
    {
        var emails = new List<string>();

        var softwares = await db.Softwares.Where(s => softwareIds.Contains(s.Id)).ToListAsync();

        foreach (var software in softwares)
        {
            var lab = await db.Labs.FindAsync(software.LabId);

            if (lab == null)
            {
                var instructors = await db.Users
                    .Where(u => u.Role == UserRole.Instructor)
                    .ToListAsync();

                emails.AddRange(instructors.Select(i => i.Email));
            }
            else
            {
                var schedule = await db.Schedules
                .Where(s => s.Lab == lab.Name && s.StartTime > DateTime.Now)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync();

                if (schedule != null)
                {
                    var user = await db.Users.FindAsync(schedule.UserId);
                    if (user != null)
                    {
                        emails.Add(user.Email);
                    }
                }
            }
        }

        return emails;
    }

    [HttpPost("send-expiration-notifications")]
    public async Task<IActionResult> SendExpirationNotifications()
    {
        try
        {
            var expiringSoftware = GetExpiringSoftware();
            var emails = await GetEmailsForSoftware(expiringSoftware.Select(s => s.Id).ToList());

            if (emails.Any())
            {
                foreach (var software in expiringSoftware)
                {
                    foreach (var email in emails)
                    {
                        var lastEmail = db.Emails
                            .Where(e => e.ToEmail == email
                                        && e.Subject == $"Software License Expiration Reminder for {software.Name}"
                                        && e.SentDate.HasValue)
                            .OrderByDescending(e => e.SentDate)
                            .FirstOrDefault();

                        if (lastEmail != null && lastEmail.SentDate.HasValue && (DateTime.Now - lastEmail.SentDate.Value).Days < 7)
                        {
                            continue;
                        }

                        var emailModel = new EmailModel
                        {
                            ToEmail = email,
                            Subject = $"Software License Expiration Reminder for {software.Name}",
                            Body = $"Your software license for '{software.Name}' is about to expire on {software.LicenseExpire?.ToString("yyyy-MM-dd")}. Please take necessary action.",
                            SentDate = DateTime.Now,
                            Status = "Sent"
                        };

                        _emailService.SendEmail(emailModel.ToEmail, emailModel.Subject, emailModel.Body);
                        db.Emails.Add(emailModel);
                        db.SaveChanges();
                    }
                }

                await db.SaveChangesAsync();
                return Ok("Expiration notifications sent successfully.");
            }

            return NotFound("No expiring software found.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}