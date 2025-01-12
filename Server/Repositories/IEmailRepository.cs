using Server.Models;

namespace Server.Repositories
{
    public interface IEmailRepository
    {

        Task AddEmailAsync(EmailModel emailModel);
        Task<EmailModel> GetLastEmailAsync(string toEmail, string subject, string name);
        Task<IEnumerable<EmailModel>> UpdateStatusAsync();
        Task<List<string>> GetInstructorEmailsForLabAsync();
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task ResendEmailIfNecessaryAsync(string toEmail, string subject, string body);
    }
}
