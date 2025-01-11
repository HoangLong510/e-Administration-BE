using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs.Schedule;
using Server.Models;
using System.Linq;

namespace Server.Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly DatabaseContext db;

        public ScheduleRepository(DatabaseContext db)
        {
            this.db = db;
        }

        public async Task<IEnumerable<Schedule>> GetAllSchedulesAsync()
        {
            return await db.Schedules.ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesByUserIdAsync(int userId)
        {
            return await db.Schedules
                .Where(s => s.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesByLabAsync(string lab)
        {
            return await db.Schedules
                .Where(s => s.Lab.ToLower().Contains(lab.ToLower()))
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesByFullNameAsync(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return Enumerable.Empty<Schedule>();
            }

            var users = await db.Users
                .Where(u => EF.Functions.Like(u.FullName.ToLower(), $"%{fullName.ToLower()}%"))
                .ToListAsync();

            if (users.Any())
            {
                var userIds = users.Select(u => u.Id).ToList();
                return await db.Schedules
                    .Where(s => userIds.Contains(s.UserId))
                    .ToListAsync();
            }

            return Enumerable.Empty<Schedule>();
        }

        public async Task<Schedule> GetScheduleByIdAsync(int id)
        {
            return await db.Schedules
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task CreateScheduleAsync(Schedule schedule)
        {
            await db.Schedules.AddAsync(schedule);
            await db.SaveChangesAsync();
        }

        public async Task DeleteScheduleAsync(int id)
        {
            var schedule = await db.Schedules.FindAsync(id);
            if (schedule != null)
            {
                db.Schedules.Remove(schedule);
                await db.SaveChangesAsync();
            }
        }
        public async Task<string> GetFullNameByUserIdAsync(int userId)
        {
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.FullName;
        }
        public async Task<User> GetUserByUserIdAsync(int userId)
        {
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user;
        }
        public async Task<Class> GetClassByIDAsync(int id)
        {
            var classUser = await db.Classes
                .FindAsync(id);
            return classUser;
        }
        public async Task<Document> CreateDocumentAsync(Document document)
        {

            await db.Documents.AddAsync(document);
            await db.SaveChangesAsync();

            return document;
        }
    }
}
