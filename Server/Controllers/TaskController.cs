using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs.Tasks;
using Server.Models;
using Server.Repositories;
using Server.Services;
using System.Text.RegularExpressions;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskRepository taskRepo;
        private readonly TokenService tokenService;

        public TaskController(ITaskRepository taskRepo, TokenService tokenService)
        {
            this.taskRepo = taskRepo;
            this.tokenService = tokenService;
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateTask(CreateTaskDTO req)
        {
            var errors = new Dictionary<string, string>();

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
                errors["assigneesId"] = "Content is required";
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
            if (!result)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to create task"
                });
            }

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
    }
}
