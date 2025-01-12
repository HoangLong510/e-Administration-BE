using Server.DTOs.Tasks;
using Server.Models;

namespace Server.Repositories
{
    public interface ITaskRepository
    {
        Task<(List<TaskResponseDto> tasks, int totalPages)> GetAllTasks(GetAllTasksRequest req);
        Task<TaskResponseDto> GetTaskById(int taskId);
        Task<Tasks> CreateTask(CreateTaskDTO req);
        Task<TaskResponseDto> ChangeTaskStatus(int taskId, int userId);
        Task<TaskResponseDto> CancelTask(int taskId, int userId);
        Task<Tasks> EditTask(EditTaskDto req);
    }
}
