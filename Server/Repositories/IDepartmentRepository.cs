using Server.Models;

namespace Server.Repositories
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> GetAllDepartmentsAsync(string? searchQuery, string? sortBy); 
        Task<Department> GetDepartmentByIdAsync(int id);
        Task<Department> CreateDepartmentAsync(Department department);
        Task<Department> UpdateDepartmentAsync(int id, Department department);
        Task<IEnumerable<Department>> GetAllDepartmentsNoPagination();

    }
}