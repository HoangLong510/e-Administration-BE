using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Repositories
{
    public class LabRepository : ILabRepository
    {
        private readonly DatabaseContext db; // Sử dụng DatabaseContext

        public LabRepository(DatabaseContext db)
        {
            this.db = db;
        }

        public async Task<IEnumerable<Lab>> GetAllLabsAsync(string? searchQuery, string? statusFilter)
        {
            try
            {
                var query = db.Labs.AsQueryable();

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query = query.Where(l => l.Name.ToLower().Contains(searchQuery.ToLower()));
                }

                if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
                {
                    bool status = statusFilter == "occupied";
                    query = query.Where(l => l.Status == status);
                }

                return await query.ToListAsync();
            }
            catch (Exception)
            {
                // Log the exception or handle it appropriately
                throw; // Re-throw the exception or return an empty list
            }
        }

        public async Task<Lab> GetLabByIdAsync(int id)
        {
            return await db.Labs.FindAsync(id);
        }

        public async Task<Lab> CreateLabAsync(Lab lab)
        {
            db.Labs.Add(lab);
            await db.SaveChangesAsync();
            return lab;
        }

        public async Task<Lab> UpdateLabAsync(int id, Lab lab)
        {
            var existingLab = await db.Labs.FindAsync(id);
            if (existingLab == null)
            {
                return null; // Hoặc throw exception
            }

            existingLab.Name = lab.Name;
            existingLab.Status = lab.Status;

            await db.SaveChangesAsync();
            return existingLab;
        }


        public async Task<(bool success, string message)> DisableLabAsync(int LabId)
        {
            var lab = await db.Labs.FirstOrDefaultAsync(l => l.Id == LabId);
            if (lab == null)
            {
                return (false, "Lab does not exist!");
            }

            lab.Status = false;
            await db.SaveChangesAsync();
            return (true, "Lab disabled successfully!");
        }

        public async Task<(int ActiveCount, int InactiveCount)> GetLabsStatusSummaryAsync()
        {
            try
            {
                int activeCount = await db.Labs.CountAsync(l => l.Status == true);
                int inactiveCount = await db.Labs.CountAsync(l => l.Status == false);
                return (activeCount, inactiveCount);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<bool> CheckNameExists(string name)
        {
            return await db.Labs.AnyAsync(l => l.Name == name);
        }


        public async Task<bool> IsLabNameUnique(string name, int labId)
        {
            return !(await db.Labs.AnyAsync(l => l.Name == name && l.Id != labId));
        }


    }
}

