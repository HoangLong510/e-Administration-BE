using Microsoft.AspNetCore.Mvc;
using Server.DTOs.Schedule;
using Server.Models;

public interface IScheduleRepository
{
    Task<IEnumerable<Schedule>> GetAllSchedulesAsync();
    Task<IEnumerable<Schedule>> GetSchedulesByUserIdAsync(int userId);
    Task<IEnumerable<Schedule>> GetSchedulesByLabAsync(string lab);
    Task<IEnumerable<Schedule>> GetSchedulesByFullNameAsync(string fullName);
    Task CreateScheduleAsync(Schedule schedule);
    Task DeleteScheduleAsync(int id);
    Task<string> GetFullNameByUserIdAsync(int userId);
    Task<Schedule> GetScheduleByIdAsync(int id);
    Task<User> GetUserByUserIdAsync(int userId);
    Task<Class> GetClassByIDAsync(int id);
    Task<IEnumerable<Class>> GetAllClassAsync();
    Task<IEnumerable<Lab>> GetAllLabAsync();
    Task<IEnumerable<GetScheduleDto>> GetScheduleByConditionAsync(string Name, string Lab);


}
