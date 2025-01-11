using Server.DTOs.Tasks;
using Server.Models;

namespace Server.Repositories
{
    public interface ITaskRepository
    {
        Task<(List<TaskResponseDto> tasks, int totalPages)> GetAllTasks(GetAllTasksRequest req);
        Task<Tasks> GetTaskById(int taskId);
        Task<bool> CreateTask(CreateTaskDTO req);
        Task<bool> ChangeTaskStatus(int taskId);
    }
}
