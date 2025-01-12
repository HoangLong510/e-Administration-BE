using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs.LabDevice;

namespace Server.Repositories
{
    public class LabDeviceRepository : ILabDeviceRepository
    {
        private readonly DatabaseContext db;

        public LabDeviceRepository(DatabaseContext db)
        {
            this.db = db;
        }

        public async Task<List<LabDeviceResponseDto>> GetAddToLab(GetLabDevicesRequestDto req)
        {
            var devices = db.Devices
                            .Where(d => d.Status == true && (d.LabId == null))
                            .AsQueryable(); // Chỉ lấy dữ liệu có Status là Active (true) và LabId là null hoặc rỗng

            var softwares = db.Softwares
                              .Where(s => s.Status == true && (s.LabId == null))
                              .AsQueryable(); // Chỉ lấy dữ liệu có Status là Active (true) và LabId là null hoặc rỗng

            if (!string.IsNullOrEmpty(req.SearchValue))
            {
                string searchValueLower = req.SearchValue.ToLower();
                devices = devices.Where(d => d.Name.ToLower().Contains(searchValueLower) || d.Description.ToLower().Contains(searchValueLower) || d.Type.ToLower().Contains(searchValueLower));
                softwares = softwares.Where(s => s.Name.ToLower().Contains(searchValueLower) || s.Description.ToLower().Contains(searchValueLower) || s.Type.ToLower().Contains(searchValueLower));
            }

            var deviceList = await devices.Select(d => new LabDeviceResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                Type = d.Type,
                Description = d.Description,
                LicenseExpire = null, // Devices do not have LicenseExpire
                IsSoftware = false
            }).ToListAsync();

            var softwareList = await softwares.Select(s => new LabDeviceResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Type = s.Type,
                Description = s.Description,
                LicenseExpire = s.LicenseExpire.HasValue ? s.LicenseExpire.Value.ToString("yyyy-MM-dd") : null,
                IsSoftware = true
            }).ToListAsync();

            return deviceList.Concat(softwareList).ToList();
        }

        public async Task<List<LabDeviceResponseDto>> GetLabDevices(GetLabDevicesRequestDto req)
        {
            var devices = db.Devices.Where(d => d.LabId == req.LabId).AsQueryable();
            var softwares = db.Softwares.Where(s => s.LabId == req.LabId).AsQueryable();

            if (!string.IsNullOrEmpty(req.SearchValue))
            {
                string searchValueLower = req.SearchValue.ToLower();
                devices = devices.Where(d => d.Name.ToLower().Contains(searchValueLower) || d.Description.ToLower().Contains(searchValueLower) || d.Type.ToLower().Contains(searchValueLower));
                softwares = softwares.Where(s => s.Name.ToLower().Contains(searchValueLower) || s.Description.ToLower().Contains(searchValueLower) || s.Type.ToLower().Contains(searchValueLower));
            }

            var deviceList = await devices.Select(d => new LabDeviceResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                Type = d.Type,
                Description = d.Description,
                LicenseExpire = null,
                IsSoftware = false,
                Status = d.Status // Include status in the response
            }).ToListAsync();

            var softwareList = await softwares.Select(s => new LabDeviceResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Type = s.Type,
                Description = s.Description,
                LicenseExpire = s.LicenseExpire.HasValue ? s.LicenseExpire.Value.ToString("yyyy-MM-dd") : null,
                IsSoftware = true,
                Status = s.Status // Include status in the response
            }).ToListAsync();

            return deviceList.Concat(softwareList).ToList();
        }


        public async Task<bool> AddDevicesToLab(int labId, List<int> deviceIds)
        {
            try
            {

                var devices = await db.Devices.Where(d => deviceIds.Contains(d.Id)).ToListAsync();
                var softwares = await db.Softwares.Where(s => deviceIds.Contains(s.Id)).ToListAsync();

             

                if (devices.Count == 0 && softwares.Count == 0)
                {
                    Console.WriteLine("No devices or softwares found to update.");
                    return false;
                }

                foreach (var device in devices)
                {
                    device.LabId = labId;
                }

                foreach (var software in softwares)
                {
                    software.LabId = labId;
                }

                

                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                Console.WriteLine($"Error in AddDevicesToLab: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> RemoveDevicesFromLab(List<int> deviceIds)
        {
            try
            {
                var devices = await db.Devices.Where(d => deviceIds.Contains(d.Id)).ToListAsync();
                var softwares = await db.Softwares.Where(s => deviceIds.Contains(s.Id)).ToListAsync();

                if (devices.Count == 0 && softwares.Count == 0)
                {
                    Console.WriteLine("No devices or softwares found to update.");
                    return false;
                }

                foreach (var device in devices)
                {
                    device.LabId = null;
                }

                foreach (var software in softwares)
                {
                    software.LabId = null;
                }

                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                Console.WriteLine($"Error in RemoveDevicesFromLab: {ex.Message}");
                return false;
            }
        }

    }
}
