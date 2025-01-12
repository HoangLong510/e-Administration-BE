using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs.Tasks;
using Server.DTOs.User;
using Server.Models;
using Server.Models.Enums;

namespace Server.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly DatabaseContext db;

        public TaskRepository(DatabaseContext db)
        {
            this.db = db;
        }

        public async Task<(List<TaskResponseDto> tasks, int totalPages)> GetAllTasks(GetAllTasksRequest req)
        {
            var pageSize = 10;
            var tasks = db.Tasks.AsQueryable();

            var user = await db.Users.SingleOrDefaultAsync(u => u.Id == req.UserId);

            if(user.Role.ToString() != "Admin")
            {
                tasks = tasks.Where(t => t.AssigneesId == user.Id);
            }

            if (!string.IsNullOrEmpty(req.Status))
            {
                tasks = tasks.Where(t => t.Status.ToString() == req.Status);
            }

            if (!string.IsNullOrEmpty(req.SearchValue))
            {
                string searchValueLower = req.SearchValue.ToLower();
                tasks = tasks.Where(t => t.Title.ToLower().Contains(searchValueLower));
            }

            var pagedTasks = await tasks
                .Include(t => t.Assignees)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Content = t.Content,
                    Assignees = t.Assignees == null ? null : new UserResponseDto
                    {
                        Id = t.Assignees.Id,
                        Username = t.Assignees.Username,
                        FullName = t.Assignees.FullName,
                        Email = t.Assignees.Email,
                        Phone = t.Assignees.Phone,
                        Address = t.Assignees.Address,
                        Gender = t.Assignees.Gender.ToString(),
                        Role = t.Assignees.Role.ToString(),
                        IsActive = t.Assignees.IsActive,
                        ClassId = t.Assignees.ClassId,
                        DepartmentId = t.Assignees.DepartmentId,
                        Avatar = t.Assignees.Avatar
                    },
                    ReportId = t.ReportId,
                    CreatedAt = t.CreatedAt,
                    ComplatedAt = t.ComplatedAt,
                    Status = t.Status.ToString()
                })
                .Skip((req.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalTasks = await tasks.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalTasks / pageSize);

            return (pagedTasks, totalPages);
        }

        public async Task<Tasks> GetTaskById(int taskId)
        {
            var task = await db.Tasks.SingleOrDefaultAsync(t => t.Id == taskId);
            return task;
        }

        public async Task<bool> CreateTask(CreateTaskDTO req)
        {
            var newTask = new Tasks
            {
                Title = req.Title,
                Content = req.Content,
                AssigneesId = req.AssigneesId,
                ReportId = req.ReportId
            };

            db.Tasks.Add(newTask);
            await db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangeTaskStatus(int taskId)
        {
            var task = await db.Tasks.SingleOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
            {
                return false;
            }

            if (task.Status.ToString() == "Pending")
            {
                task.Status = TaskStatusEnum.InProgress;
            }
            else if (task.Status.ToString() == "InProgress")
            {
                task.Status = TaskStatusEnum.Completed;
            }
            else
            {
                return false;
            }

            return false;
        }

        public async Task<List<Tasks>> GetTaskByReportId(int reportId)
        {
            return await db.Tasks
                                 .Where(u => u.ReportId == reportId)
                                 .Include(t => t.Assignees)
                                 .ToListAsync();
        }
    }
}
