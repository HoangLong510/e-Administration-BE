using Server.Models;

namespace Server.Repositories
{
    public interface INotificationRepository
    {
        Task<bool> CreateNotiReportAsync(int ReportId,Notification notification);
        Task<bool> CreateNotiTaskAsync(int TaskId,Notification notification);
        Task<List<Notification>> ListNotiAsync(int userId);

    }
}