using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs.Tasks;
using Server.Models;
using Server.Models.Enums;
using Server.Repositories;
using Server.Services;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskRepository taskRepo;
        private readonly INotificationRepository notiRepo;
        private readonly IUserRepository userRepo;
        private readonly TokenService tokenService;

        public TaskController(ITaskRepository taskRepo, INotificationRepository notiRepo, IUserRepository userRepo, TokenService tokenService)
        {
            this.taskRepo = taskRepo;
            this.notiRepo = notiRepo;
            this.userRepo = userRepo;
            this.tokenService = tokenService;
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateTask(CreateTaskDTO req)
        {
            var errors = new Dictionary<string, string>();
            var token = Request.Cookies["token"];
            var userId = int.Parse(tokenService.GetUserIdFromToken(token));

            // check title
            if (string.IsNullOrWhiteSpace(req.Title))
            {
                errors["title"] = "Title is required";
            }

            // check title
            if (string.IsNullOrWhiteSpace(req.Content))
            {
                errors["content"] = "Content is required";
            }

            // check AssigneesId
            if (req.AssigneesId == 0)
            {
                errors["assigneesId"] = "Assignees is required";
            }

            if (errors.Count > 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Errors = errors,
                    Message = "Invalid task information! Please check the errors of the fields again."
                });
            }

            var result = await taskRepo.CreateTask(req);
            if (result == null)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to create task"
                });
            }

            var notification = new Notification
            {
                SenderId = userId,
                ReceiverId = req.AssigneesId,
                TaskId = result.Id,
                Content = "You have just been assigned a new task",
                ActionType = "NewTask",
                CreatedAt = DateTime.Now
            };

            await notiRepo.CreateNotiTaskAsync(notification);

            return Ok(new
            {
                Success = true,
                Message = "Create task successfully"
            });
        }

        [HttpPost("get-tasks")]
        public async Task<ActionResult> GetTasks(GetAllTasksRequest req)
        {
            var token = Request.Cookies["token"];
            var userId = tokenService.GetUserIdFromToken(token);
            req.UserId = int.Parse(userId);

            var (tasks, totalPages) = await taskRepo.GetAllTasks(req);

            return Ok(new
            {
                Success = true,
                Tasks = tasks,
                TotalPages = totalPages
            });
        }

        [HttpGet("get-task-by-id/{taskId}")]
        public async Task<ActionResult> GetTaskById(int taskId)
        {
            var token = Request.Cookies["token"];
            var userId = int.Parse(tokenService.GetUserIdFromToken(token));
            var userRole = tokenService.GetRoleFromToken(token);

            var task = await taskRepo.GetTaskById(taskId);

            if(task == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Task not found"
                });
            }

            if(userRole != "Admin")
            {
                if(userId != task.Assignees.Id)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "You are not authorized to view this task"
                    });
                }
            }

            return Ok(new
            {
                Success = true,
                Data = task
            });
        }

        [HttpGet("change-task-status/{taskId}")]
        public async Task<ActionResult> ChangeTaskStatus(int taskId)
        {
            var token = Request.Cookies["token"];
            var userId = int.Parse(tokenService.GetUserIdFromToken(token));

            var task = await taskRepo.ChangeTaskStatus(taskId, userId);

            if (task == null)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to change task status"
                });
            }

            var admins = await userRepo.GetUsersByRoleAsync(UserRole.Admin);

            foreach (var admin in admins)
            {
                var notification = new Notification
                {
                    SenderId = userId,
                    ReceiverId = admin.Id,
                    Content = $"Assignees just changed the status of the task with id {task.Id}",
                    TaskId = task.Id,
                    ActionType = "ChangeStatusTask",
                    CreatedAt = DateTime.Now
                };

                await notiRepo.CreateNotiTaskAsync(notification);
            }

            return Ok(new
            {
                Success = true,
                Message = "Change task status successfully",
                Data = task
            });
        }

        [HttpGet("cancel-task/{taskId}")]
        public async Task<ActionResult> CancelTask(int taskId)
        {
            var token = Request.Cookies["token"];
            var userId = int.Parse(tokenService.GetUserIdFromToken(token));
            var task = await taskRepo.CancelTask(taskId, userId);

            if (task == null)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to cancel task"
                });
            }

            var notification = new Notification
            {
                SenderId = userId,
                ReceiverId = task.Assignees.Id,
                Content = $"Task with id {task.Id} has just been canceled",
                TaskId = task.Id,
                ActionType = "CancelTask",
                CreatedAt = DateTime.Now
            };

            await notiRepo.CreateNotiTaskAsync(notification);

            return Ok(new
            {
                Success = true,
                Message = "Cancel task successfully",
                Data = task
            });
        }

        [HttpPost("edit-task")]
        public async Task<ActionResult> EditTask(EditTaskDto req)
        {
            var errors = new Dictionary<string, string>();
            var token = Request.Cookies["token"];
            var userId = int.Parse(tokenService.GetUserIdFromToken(token));
            req.UserId = userId;

            // check title
            if (string.IsNullOrWhiteSpace(req.Title))
            {
                errors["title"] = "Title is required";
            }

            // check title
            if (string.IsNullOrWhiteSpace(req.Content))
            {
                errors["content"] = "Content is required";
            }

            // check AssigneesId
            if (req.AssigneesId == 0)
            {
                errors["assigneesId"] = "Assignees is required";
            }

            if (errors.Count > 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Errors = errors,
                    Message = "Invalid task information! Please check the errors of the fields again."
                });
            }

            var result = await taskRepo.EditTask(req);

            if (result == null)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to edit task"
                });
            }

            var notification = new Notification
            {
                SenderId = userId,
                ReceiverId = result.Assignees.Id,
                Content = $"Task with id {result.Id} has just been edited",
                TaskId = result.Id,
                ActionType = "EditTask",
                CreatedAt = DateTime.Now
            };

            await notiRepo.CreateNotiTaskAsync(notification);

            return Ok(new
            {
                Success = true,
                Message = "Edit task successfully"
            });
        }
    }
}
