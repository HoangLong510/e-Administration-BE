using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        public Task<bool> CreateNotiReportAsync(int ReportId, Notification notification)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateNotiTaskAsync(int TaskId, Notification notification)
        {
            throw new NotImplementedException();
        }

        public Task<List<Notification>> ListNotiAsync(int userId)
        {
            throw new NotImplementedException();
        }
    }
}