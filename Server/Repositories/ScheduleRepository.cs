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

            var scheduleList = await (from s in db.Schedules
                                      join l in db.Labs on s.Lab equals l.Name
                                      where l.Status == true
                                      select s)
                                     .ToListAsync();

            return scheduleList;
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
            var schedule = await db.Schedules
                 .FirstOrDefaultAsync(s => s.Id == id);
            return schedule;
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
        public async Task<IEnumerable<Class>> GetAllClassAsync()
        {
            return await db.Classes.ToListAsync();
        }
        public async Task<IEnumerable<Lab>> GetAllLabAsync()
        {
            return await db.Labs.ToListAsync();
        }
        public async Task<IEnumerable<GetScheduleDto>> GetScheduleByConditionAsync(string Name, string Lab)
        {
            // Query to join schedules and users
            var query = from schedule in db.Schedules
                        join user in db.Users on schedule.UserId equals user.Id
                        select new { schedule, user };
            if (!string.IsNullOrEmpty(Name))
            {
                query = query.Where(x => x.user.FullName.Contains(Name));
            }
            if (!string.IsNullOrEmpty(Lab))
            {
                query = query.Where(x => x.schedule.Lab.Contains(Lab));
            }
            var result = await query.Select(x => new GetScheduleDto
            {
                Id = x.schedule.Id,
                Course = x.schedule.Course,
                Lab = x.schedule.Lab,
                Class = x.schedule.Class,
                StartTime = x.schedule.StartTime,
                EndTime = x.schedule.EndTime,
                UserId = x.schedule.UserId,
                FullName = x.user.FullName
            }).ToListAsync();

            return result;
        }


    }
}
