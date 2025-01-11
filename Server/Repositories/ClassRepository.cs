using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using System.Security.Claims;

namespace Server.Repositories
{
    public class ClassRepository : IClassRepository
    {
        private readonly DatabaseContext db;

        public ClassRepository(DatabaseContext db)
        {
            this.db = db;
        }

        public async Task<IEnumerable<Class>> GetAllClassesAsync()
        {
            return await db.Classes.ToListAsync();
        }

        public async Task<(IEnumerable<Class> Classes, int TotalCount)> GetPagedClassesAsync(string search, int page, int pageSize)
        {
            var query = db.Classes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Name.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var classes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (classes, totalCount);
        }

        public async Task<Class> GetClassByIdAsync(int id)
        {
            return await db.Classes.FindAsync(id);
        }

        public async Task AddClassAsync(Class newClass)
        {
            await db.Classes.AddAsync(newClass);
            await db.SaveChangesAsync();
        }

        public async Task UpdateClassAsync(Class updatedClass)
        {
            var existingClass = await db.Classes.FindAsync(updatedClass.Id);
            if (existingClass != null)
            {
                db.Entry(existingClass).State = EntityState.Detached;
            }

            db.Classes.Update(updatedClass);
            await db.SaveChangesAsync();
        }

        public async Task DeleteClassAsync(int id)
        {
            var classToDelete = await db.Classes.FindAsync(id);
            if (classToDelete != null)
            {
                db.Classes.Remove(classToDelete);
                await db.SaveChangesAsync();
            }
        }

        public async Task<bool> ClassNameExistsAsync(string className)
        {
            return await db.Classes.AnyAsync(c => c.Name == className);
        }

        public async Task<List<User>> GetUsersByClassIdAsync(int classId)
        {
            return await db.Users
                                 .Where(u => u.ClassId == classId && u.IsActive)
                                 .ToListAsync();
        }

    }
}
