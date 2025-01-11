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

        public Task<bool> CreateNotiTaskAsync(Notification notification)
        {
            throw new NotImplementedException();
        }

        public Task<List<Notification>> ListNotiAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MarkAsViewedAsync(int notificationId)
        {
            throw new NotImplementedException();
        }
    }
}