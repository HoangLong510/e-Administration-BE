using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.DTOs.Device;
using Server.Repositories;
using Server.Services;

namespace Server.Controllers
{
    [Route("api/management/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceRepository deviceRepo;
        private readonly TokenService tokenService;

        public DeviceController(IDeviceRepository deviceRepo, TokenService tokenService)
        {
            this.deviceRepo = deviceRepo;
            this.tokenService = tokenService;
        }

        [HttpPost("get-devices")]
        public async Task<ActionResult> GetDevices( GetDevicesRequestDto req)
        {
            var token = Request.Cookies["token"];
            var role = tokenService.GetRoleFromToken(token);
            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "You do not have permission to perform this action"
                });
            }

            var (devices, totalPages) = await deviceRepo.GetDevices(req);

            return Ok(new
            {
                Success = true,
                Devices = devices,
                TotalPages = totalPages
            });
        }
        [HttpGet("get-device/{id}")]
        public async Task<ActionResult> GetDeviceById(int id)
        {
            var token = Request.Cookies["token"];
            var role = tokenService.GetRoleFromToken(token);
            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "You do not have permission to perform this action"
                });
            }

            var device = await deviceRepo.GetDeviceById(id);
            if (device == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Device not found"
                });
            }

            return Ok(new
            {
                Success = true,
                Device = new
                {
                    device.Id,
                    device.LabId,
                    device.Name,
                    device.Type,
                    device.Description,
                    device.Image,
                    device.Status
                }
            });
        }

        [HttpPost("create-device")]
        public async Task<ActionResult> CreateDevice([FromForm] DeviceCreateDto device)
        {
            var token = Request.Cookies["token"];
            var role = tokenService.GetRoleFromToken(token);
            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "You do not have permission to perform this action"
                });
            }

            var errors = new Dictionary<string, string>();

            // Kiểm tra các trường bắt buộc không được để trống
            if (string.IsNullOrWhiteSpace(device.Name))
            {
                errors["name"] = "Name is required";
            }

            if (string.IsNullOrWhiteSpace(device.Type))
            {
                errors["type"] = "Type is required";
            }

            if (string.IsNullOrWhiteSpace(device.Description))
            {
                errors["description"] = "Description is required";
            }

            if (device.ImageFile == null)
            {
                errors["imageFile"] = "Image file is required";
            }

            if (errors.Count > 0)
            {
                return BadRequest(new
                {
                    Success = false,
                    Errors = errors,
                    Message = "Invalid device information! Please check the errors of the fields again."
                });
            }

            try
            {
                var result = await deviceRepo.CreateDevice(device);
                if (!result)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Create device failed!"
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Device created successfully!"
                });
            }
            catch (Exception ex)
            {
                // Ghi log chi tiết lỗi
                Console.Error.WriteLine($"Error creating device: {ex.Message}");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while creating the device."
                });
            }
        }





        [HttpGet("disable-device/{id}")]
        public async Task<ActionResult> DisableDevice(int id)
        {
            var token = Request.Cookies["token"];
            var role = tokenService.GetRoleFromToken(token);
            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "You do not have permission to perform this action"
                });
            }

            var (result, message) = await deviceRepo.DisableDevice(id);
            if (!result)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = message
                });
            }

            return Ok(new
            {
                Success = true,
                Message = message
            });
        }

        [HttpPut("update-device/{id}")]
        public async Task<ActionResult> UpdateDevice(int id, [FromForm] DeviceUpdateDto request)
        {
            var token = Request.Cookies["token"];
            var role = tokenService.GetRoleFromToken(token);
            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "You do not have permission to perform this action"
                });
            }

            var result = await deviceRepo.UpdateDevice(id, request);
            if (!result)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to update device"
                });
            }

            return Ok(new
            {
                Success = true,
                Message = "Device updated successfully"
            });
        }

    }
}
