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
            else
            {
                tasks = tasks.Where(t => t.Status.ToString() != "Canceled");
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

        public async Task<TaskResponseDto> GetTaskById(int taskId)
        {
            var task = await db.Tasks.Include(t => t.Assignees).SingleOrDefaultAsync(t => t.Id == taskId);
            if(task == null)
            {
                return null;
            }
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Content = task.Content,
                Assignees = task.Assignees == null ? null : new UserResponseDto
                {
                    Id = task.Assignees.Id,
                    Username = task.Assignees.Username,
                    FullName = task.Assignees.FullName,
                    Email = task.Assignees.Email,
                    Phone = task.Assignees.Phone,
                    Address = task.Assignees.Address,
                    Gender = task.Assignees.Gender.ToString(),
                    Role = task.Assignees.Role.ToString(),
                    IsActive = task.Assignees.IsActive,
                    ClassId = task.Assignees.ClassId,
                    DepartmentId = task.Assignees.DepartmentId,
                    Avatar = task.Assignees.Avatar
                },
                ReportId = task.ReportId,
                CreatedAt = task.CreatedAt,
                ComplatedAt = task.ComplatedAt,
                Status = task.Status.ToString()
            };
        }

        public async Task<Tasks> CreateTask(CreateTaskDTO req)
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

            return newTask;
        }

        public async Task<TaskResponseDto> ChangeTaskStatus(int taskId, int userId)
        {
            var task = await db.Tasks.Include(t => t.Assignees).SingleOrDefaultAsync(t => t.Id == taskId);
            var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId);

            if (task == null)
            {
                return null;
            }

            if(user == null)
            {
                return null;
            }

            if(user.Role.ToString() != "Admin")
            {
                if(user.Id != task.AssigneesId)
                {
                    return null;
                }
            }

            if (task.Status.ToString() == "Pending")
            {
                task.Status = TaskStatusEnum.InProgress;
            }
            else if (task.Status.ToString() == "InProgress")
            {
                task.Status = TaskStatusEnum.Completed;
                task.ComplatedAt = DateTime.Now;
            }
            else
            {
                return null;
            }

            await db.SaveChangesAsync();

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Content = task.Content,
                Assignees = task.Assignees == null ? null : new UserResponseDto
                {
                    Id = task.Assignees.Id,
                    Username = task.Assignees.Username,
                    FullName = task.Assignees.FullName,
                    Email = task.Assignees.Email,
                    Phone = task.Assignees.Phone,
                    Address = task.Assignees.Address,
                    Gender = task.Assignees.Gender.ToString(),
                    Role = task.Assignees.Role.ToString(),
                    IsActive = task.Assignees.IsActive,
                    ClassId = task.Assignees.ClassId,
                    DepartmentId = task.Assignees.DepartmentId,
                    Avatar = task.Assignees.Avatar
                },
                ReportId = task.ReportId,
                CreatedAt = task.CreatedAt,
                ComplatedAt = task.ComplatedAt,
                Status = task.Status.ToString()
            };
        }

        public async Task<TaskResponseDto> CancelTask(int taskId, int userId)
        {
            var task = await db.Tasks.Include(t => t.Assignees).SingleOrDefaultAsync(t => t.Id == taskId);
            var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId);

            if (task == null)
            {
                return null;
            }

            if (user == null)
            {
                return null;
            }

            if (user.Role.ToString() != "Admin")
            {
                return null;
            }

            task.Status = TaskStatusEnum.Canceled;

            await db.SaveChangesAsync();

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Content = task.Content,
                Assignees = task.Assignees == null ? null : new UserResponseDto
                {
                    Id = task.Assignees.Id,
                    Username = task.Assignees.Username,
                    FullName = task.Assignees.FullName,
                    Email = task.Assignees.Email,
                    Phone = task.Assignees.Phone,
                    Address = task.Assignees.Address,
                    Gender = task.Assignees.Gender.ToString(),
                    Role = task.Assignees.Role.ToString(),
                    IsActive = task.Assignees.IsActive,
                    ClassId = task.Assignees.ClassId,
                    DepartmentId = task.Assignees.DepartmentId,
                    Avatar = task.Assignees.Avatar
                },
                ReportId = task.ReportId,
                CreatedAt = task.CreatedAt,
                ComplatedAt = task.ComplatedAt,
                Status = task.Status.ToString()
            };
        }

        public async Task<Tasks> EditTask(EditTaskDto req)
        {
            var task = await db.Tasks.Include(t => t.Assignees).SingleOrDefaultAsync(t => t.Id == req.Id);
            if (task == null)
            {
                return null;
            }
            var user = await db.Users.SingleOrDefaultAsync(u => u.Id == req.UserId);

            if(user == null)
            {
                return null;
            }

            if(user.Role.ToString() != "Admin")
            {
                return null;
            }

            task.Title = req.Title;
            task.Content = req.Content;
            task.AssigneesId = req.AssigneesId;

            await db.SaveChangesAsync();

            return task;
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
