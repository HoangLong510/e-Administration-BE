using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly DatabaseContext db;

        public DepartmentRepository(DatabaseContext db)
        {
            this.db = db;
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync(string? searchQuery, string? sortBy)
        {
            try
            {
                var query = db.Departments.AsQueryable();

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query = query.Where(d => d.Name.ToLower().Contains(searchQuery.ToLower()));
                }

                if (!string.IsNullOrEmpty(sortBy))
                {
                    switch (sortBy)
                    {
                        case "name_asc":
                            query = query.OrderBy(d => d.Name);
                            break;
                        case "name_desc":
                            query = query.OrderByDescending(d => d.Name);
                            break;
                    }
                }

                var departments= await query.ToListAsync();
                foreach (var department in departments)
                {
                    department.User= await db.Users.SingleOrDefaultAsync(u => u.Id == department.Hod);
                }
                return departments;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllDepartmentsAsync: {ex.Message}");
                return new List<Department>();
            }
        }

        public async Task<Department> GetDepartmentByIdAsync(int id)
        {
            return await db.Departments.FindAsync(id);
        }

        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            db.Departments.Add(department);
            await db.SaveChangesAsync();
            return department;
        }

        public async Task<Department> UpdateDepartmentAsync(int id, Department department)
        {
            var existingDepartment = await db.Departments.FindAsync(id);
            if (existingDepartment == null)
            {
                return null;
            }

            existingDepartment.Name = department.Name;
            existingDepartment.Hod = department.Hod;
            existingDepartment.Description = department.Description;

            await db.SaveChangesAsync();
            return existingDepartment;
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsNoPagination()
        {
            var departments = await db.Departments.ToListAsync();
            return departments;
        }

    }
}