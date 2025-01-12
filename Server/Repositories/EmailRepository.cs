using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Server.Models.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Repositories
{
    public class EmailRepository : IEmailRepository
    {
        private readonly DatabaseContext _context;
        private readonly EmailService _emailService;

        public EmailRepository(DatabaseContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task AddEmailAsync(EmailModel emailModel)
        {
            _context.Emails.Add(emailModel);
            await _context.SaveChangesAsync();
        }

        public async Task<EmailModel> GetLastEmailAsync(string toEmail, string subject)
        {
            return await _context.Emails
                .Where(email => email.ToEmail == toEmail && email.Subject == subject)
                .OrderByDescending(email => email.SentDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<EmailModel>> UpdateStatusAsync()
        {
            var emails = await _context.Emails.ToListAsync();
            foreach (var email in emails)
            {
                email.Status = "sent";
            }

            await _context.SaveChangesAsync();
            return emails;
        }

        public async Task<List<string>> GetInstructorEmailsForLabAsync()
        {
            var currentDate = DateTime.UtcNow;

            var softwareList = await _context.Softwares
                .Where(s => s.LicenseExpire.HasValue && s.LicenseExpire.Value <= currentDate.AddMonths(1))
                .ToListAsync();

            var emails = new List<string>();

            foreach (var software in softwareList)
            {
                if (software.LabId == null)
                {
                    var users = await _context.Users
                        .Where(u => u.Role == UserRole.Instructor && u.IsActive)
                        .ToListAsync();
                    emails.AddRange(users.Select(u => u.Email).Where(e => !string.IsNullOrEmpty(e)));
                }
                else
                {
                    var lab = await _context.Labs
                        .Include(l => l.Softwares)
                        .FirstOrDefaultAsync(l => l.Id == software.LabId);

                    if (lab != null)
                    {
                        var scheduleUsers = await _context.Schedules
                            .Where(s => s.Lab == lab.Name)
                            .Select(s => s.UserId)
                            .ToListAsync();

                        var userEmails = await _context.Users
                            .Where(u => scheduleUsers.Contains(u.Id) && u.Role == UserRole.Instructor && u.IsActive)
                            .Select(u => u.Email)
                            .ToListAsync();

                        emails.AddRange(userEmails.Where(e => !string.IsNullOrEmpty(e)));
                    }
                }
            }

            return emails;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var lastEmail = await _context.Emails
                    .Where(email => email.ToEmail == toEmail && email.Subject == subject && email.Status == "sent")
                    .OrderByDescending(email => email.SentDate)
                    .FirstOrDefaultAsync();

                if (lastEmail == null || (lastEmail.SentDate.HasValue && lastEmail.SentDate.Value <= DateTime.UtcNow.AddDays(-7)))
                {
                    var user = await _context.Users
                        .Where(u => u.Email == toEmail)
                        .FirstOrDefaultAsync();

                    var software = await _context.Softwares
                        .FirstOrDefaultAsync(s => s.LicenseExpire.HasValue && s.LicenseExpire.Value <= DateTime.UtcNow.AddMonths(1));

                    var softwareName = software?.Name ?? "Software Name";
                    var licenseExpire = software?.LicenseExpire?.ToString("yyyy-MM-dd") ?? "Expire Date";

                    var emailBody = $@"
                    Dear {user?.FullName ?? "User"},

                    This email is sent from the e-Administration system to inform you that your software license for:
                    - **Software**: {softwareName}
                    - **License Expiration Date**: {licenseExpire}

                    is about to expire. Kindly notify the Admin or Technical Support team to take the necessary actions.

                    Thank you for your attention.

                    Best regards,
                    e-Administration Team
                    ";

                    _emailService.SendEmail(toEmail, subject, emailBody);

                    var emailModel = new EmailModel
                    {
                        ToEmail = toEmail,
                        Subject = subject,
                        Body = emailBody,
                        Status = "sent",
                        SentDate = DateTime.UtcNow
                    };

                    await AddEmailAsync(emailModel);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending email to {toEmail}: {ex.Message}");
            }
        }

        public async Task ResendEmailIfNecessaryAsync(string toEmail, string subject, string body)
        {
            var currentDate = DateTime.UtcNow;

            var lastEmail = await _context.Emails
                .Where(email => email.ToEmail == toEmail && email.Subject == subject && email.Status == "sent")
                .OrderByDescending(email => email.SentDate)
                .FirstOrDefaultAsync();

            if (lastEmail == null || (lastEmail.SentDate.HasValue && lastEmail.SentDate.Value <= currentDate.AddDays(-7)))
            {
                await SendEmailAsync(toEmail, subject, body);
            }
        }
    }
}
