using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs.Device;
using Server.Models;
using System;

namespace Server.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly DatabaseContext db;
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Uploads");

        public DeviceRepository(DatabaseContext db)
        {
            this.db = db;
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(_uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }
            return fileName;
        }

        public async Task<Device> GetDeviceById(int deviceId)
        {
            var device = await db.Devices.SingleOrDefaultAsync(u => u.Id == deviceId);
            if (device != null)
            {
                return device;
            }
            return null;
        }

        public async Task<(List<DeviceResponseDto> devices, int totalPages)> GetDevices(GetDevicesRequestDto req)
        {
            var pageSize = 10;  // Sử dụng giá trị PageSize từ request
            var devices = db.Devices.AsQueryable();

            if (req.Status.HasValue)
            {
                devices = devices.Where(d => d.Status == req.Status);
            }

            // Thực hiện tìm kiếm dựa trên giá trị tìm kiếm
            if (!string.IsNullOrEmpty(req.SearchValue))
            {
                string searchValueLower = req.SearchValue.ToLower();
                devices = devices.Where(d => d.Name.ToLower().Contains(searchValueLower) ||
                                             d.Description.ToLower().Contains(searchValueLower) ||
                                             d.Type.ToLower().Contains(searchValueLower));
            }

            // Lấy danh sách thiết bị đã phân trang
            var pageDevices = await devices
                .Skip((req.PageNumber - 1) * pageSize)  // Skip các thiết bị đã qua
                .Take(pageSize)                        // Lấy số lượng thiết bị trong 1 trang
                .ToListAsync();

            var totalDevice = await devices.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalDevice / pageSize);

            // Chuyển đổi các thiết bị thành DeviceResponseDto
            var result = new List<DeviceResponseDto>();
            foreach (var device in pageDevices)
            {
                result.Add(new DeviceResponseDto
                {
                    Id = device.Id,
                    LabId = device.LabId,
                    Name = device.Name,
                    Type = device.Type,
                    Description = device.Description,
                    Image = device.Image,
                    Status = device.Status,
                });
            }

            return (result, totalPages);
        }

        public async Task<bool> CreateDevice(DeviceCreateDto device)
        {
            var imagePath = await SaveImage(device.ImageFile);
            var newDevice = new Device
            {
                LabId = device.LadID,
                Name = device.Name,
                Type = device.Type,
                Description = device.Description,
                Image = imagePath,
                Status = device.Status
            };

            db.Devices.Add(newDevice);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<(bool success, string message)> DisableDevice(int deviceId)
        {
            var device = await db.Devices.FirstOrDefaultAsync(d => d.Id == deviceId);
            if (device == null)
            {
                return (false, "Device does not exist!");
            }

            device.Status = false;
            await db.SaveChangesAsync();
            return (true, "Disable device successfully!");
        }

        public async Task<bool> UpdateDevice(int deviceId, DeviceUpdateDto request)
        {
            var device = await db.Devices.SingleOrDefaultAsync(u => u.Id == deviceId);
            if (device == null)
            {
                return false;
            }

            device.Name = request.Name;
            device.Type = request.Type;
            device.Description = request.Description;
            device.Status = request.Status;

            if (request.ImageFile != null)
            {
                // Xóa hình ảnh cũ nếu có
                if (!string.IsNullOrEmpty(device.Image) && File.Exists(device.Image))
                {
                    File.Delete(device.Image);
                }

                // Lưu hình ảnh mới
                var imagePath = await SaveImage(request.ImageFile);
                device.Image = imagePath;
            }

            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckNameExists(string name)
        {
            return await db.Devices.AnyAsync(s => s.Name == name);
        }

        public async Task<bool> IsDeviceNameUnique(string name, int deviceId)
        {
            return !(await db.Devices.AnyAsync(s => s.Name == name && s.Id != deviceId));
        }
    }
}
