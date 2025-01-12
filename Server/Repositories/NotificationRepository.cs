using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly DatabaseContext db;
        public NotificationRepository(DatabaseContext db)
        {
            this.db = db;
        }
        public async Task<bool> CreateNotiReportAsync(Notification notification)
        {
            db.Notifications.Add(notification);
            var result = await db.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> CreateNotiTaskAsync(Notification notification)
        {
            db.Notifications.Add(notification);
            var result = await db.SaveChangesAsync();
            return result > 0;
        }

        public async Task<(List<Notification> Notifications, int UnreadCount)> ListNotiAsync(int userId)
        {
            var unreadCount = await db.Notifications.CountAsync(n => !n.Viewed);
            var notifications = await db.Notifications
             .Where(n => n.ReceiverId == userId)
             .OrderByDescending(n => n.CreatedAt)
             .ToListAsync();
            return (notifications, unreadCount);
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
        {
            var notification = await db.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId);
            if (notification == null)
            {
                return false; // Không tìm thấy thông báo
            }

            notification.Viewed = true; // Cập nhật trạng thái thành đã xem
            db.Notifications.Update(notification);
            await db.SaveChangesAsync();

            return true; // Trả về thành công
        }
    }  
}