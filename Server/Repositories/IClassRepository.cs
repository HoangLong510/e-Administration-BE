using Server.Models;
using System.Security.Claims;

namespace Server.Repositories
{
    public interface IClassRepository
    {
        Task<IEnumerable<Class>> GetAllClassesAsync();
        Task<Class> GetClassByIdAsync(int id);
        Task AddClassAsync(Class newClass);
        Task UpdateClassAsync(Class updatedClass);
        Task DeleteClassAsync(int id);
        Task<(IEnumerable<Class> Classes, int TotalCount)> GetPagedClassesAsync(string search, int page, int pageSize);
    }
}
