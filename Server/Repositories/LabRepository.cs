﻿using Microsoft.EntityFrameworkCore;
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

        public async Task<bool> DeleteLabAsync(int id)
        {
            var lab = await db.Labs.FindAsync(id);
            if (lab == null)
            {
                return false; // Hoặc throw exception
            }

            db.Labs.Remove(lab);
            await db.SaveChangesAsync();
            return true;
        }
    }
}