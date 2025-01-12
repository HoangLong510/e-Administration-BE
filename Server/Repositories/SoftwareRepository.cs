using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs.Software;
using Server.Models;
using System;
using Server.DTOs.Software.Server.DTOs.Software;

namespace Server.Repositories
{
    public class SoftwareRepository : ISoftwareRepository
    {
        private readonly DatabaseContext db;

        public SoftwareRepository(DatabaseContext db)
        {
            this.db = db;
        }

        public async Task<Software> GetSoftwareById(int softwareId)
        {
            var software = await db.Softwares.SingleOrDefaultAsync(u => u.Id == softwareId);
            if (software != null)
            {
                return software;
            }
            return null;
        }

        public async Task<(List<SoftwareResponseDto> softwares, int totalPages)> GetSoftwares(GetSoftwaresRequestDto req)
        {
            var pageSize = 10;
            var softwares = db.Softwares.AsQueryable();

            if (req.Status.HasValue)
            {
                softwares = softwares.Where(d => d.Status == req.Status);
            }

            if (!string.IsNullOrEmpty(req.SearchValue))
            {
                string searchValueLower = req.SearchValue.ToLower();
                softwares = softwares.Where(d => d.Name.ToLower().Contains(searchValueLower) ||
                                                 d.Description.ToLower().Contains(searchValueLower));
            }

            var pageSoftwares = await softwares
                .Skip((req.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalSoftware = await softwares.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalSoftware / pageSize);

            var result = new List<SoftwareResponseDto>();
            foreach (var software in pageSoftwares)
            {
                result.Add(new SoftwareResponseDto
                {
                    Id = software.Id,
                    LabId = software.LabId,
                    Type = software.Type,
                    Name = software.Name,
                    Description = software.Description,
                    LicenseExpire = software.LicenseExpire,
                    Status = software.Status,
                });
            }

            return (result, totalPages);
        }

        public async Task<bool> CreateSoftware(SoftwareCreateDto software)
        {
            var newSoftware = new Software
            {
                LabId = software.LabId,
                Name = software.Name,
                Description = software.Description,
                Type = "Software",
                LicenseExpire = DateTime.Parse(software.LicenseExpire),
                Status = software.Status
            };

            db.Softwares.Add(newSoftware);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<(bool success, string message)> DisableSoftware(int softwareId)
        {
            var software = await db.Softwares.FirstOrDefaultAsync(d => d.Id == softwareId);
            if (software == null)
            {
                return (false, "Software does not exist!");
            }

            software.Status = false;
            await db.SaveChangesAsync();
            return (true, "Disable software successfully!");
        }

        public async Task<bool> UpdateSoftware(int softwareId, SoftwareUpdateDto request)
        {
            var software = await db.Softwares.SingleOrDefaultAsync(u => u.Id == softwareId);
            if (software == null)
            {
                return false;
            }

            // Check if the licenseExpire date has been changed
            if (!string.IsNullOrWhiteSpace(request.LicenseExpire) && software.LicenseExpire != DateTime.Parse(request.LicenseExpire))
            {
                var newLicenseExpireDate = DateTime.Parse(request.LicenseExpire);
                var currentDate = DateTime.Now.Date;

                // Validate the new licenseExpire date
                if (newLicenseExpireDate < currentDate)
                {
                    throw new InvalidOperationException("License Expire date cannot be earlier than today.");
                }

                software.LicenseExpire = newLicenseExpireDate;
            }

            software.Name = request.Name;
            software.Description = request.Description;
            software.Status = request.Status;

            await db.SaveChangesAsync();
            return true;
        }




        public async Task<int> CountExpiredSoftware()
        {
            var currentDate = DateTime.Now.Date;
            var count = await db.Softwares
                .Where(s => s.LicenseExpire.HasValue &&
                            s.Status == true &&
                            s.LicenseExpire.Value >= currentDate &&
                            s.LicenseExpire.Value <= currentDate.AddDays(30))
                .CountAsync();
            return count;
        }

        public async Task<bool> CheckNameExists(string name)
        {
            return await db.Softwares.AnyAsync(s => s.Name == name);
        }

        public async Task<bool> IsSoftwareNameUnique(string name, int softwareId)
        {
            return !(await db.Softwares.AnyAsync(s => s.Name == name && s.Id != softwareId));
        }

        
        public async Task UpdateStatusForExpiredLicenses()
        {
            var currentDate = DateTime.Now;
            var expiredSoftwares = await db.Softwares
                .Where(s => s.LicenseExpire.HasValue && s.LicenseExpire.Value < currentDate && s.Status == true)
                .ToListAsync();

            foreach (var software in expiredSoftwares)
            {
                software.Status = false;
            }

            await db.SaveChangesAsync();
        }
    }
}
