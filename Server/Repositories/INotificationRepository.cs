using Server.Models;

namespace Server.Repositories
{
    public interface INotificationRepository
    {
        Task<bool> CreateNotiReportAsync(Notification notification);
        Task<bool> CreateNotiTaskAsync(Notification notification);
        Task<(List<Notification> Notifications ,int UnreadCount)> ListNotiAsync(int userId);
        Task<bool> MarkNotificationAsReadAsync(int notificationId);
    }
}