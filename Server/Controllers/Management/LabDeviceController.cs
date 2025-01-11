using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs;
using Server.DTOs.LabDevice;
using Server.Repositories;
using Server.Services;

namespace Server.Controllers
{
    [Route("api/management/[controller]")]
    [ApiController]
    public class LabDeviceController : ControllerBase
    {
        private readonly ILabDeviceRepository _labDeviceRepository;
        private readonly TokenService _tokenService;

        public LabDeviceController(ILabDeviceRepository LabDeviceRepository, TokenService tokenService)
        {
            _labDeviceRepository = LabDeviceRepository;
            _tokenService = tokenService;
        }

        [HttpPost("get-add-to-lab")]
        public async Task<ActionResult> GetAddToLab(GetLabDevicesRequestDto req)
        {
            var token = Request.Cookies["token"];
            var role = _tokenService.GetRoleFromToken(token);
            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "You do not have permission to perform this action"
                });
            }

            var devices = await _labDeviceRepository.GetAddToLab(req);
            return Ok(new
            {
                Success = true,
                Devices = devices
            });
        }

        [HttpPost("get-lab-devices")]
        public async Task<ActionResult> GetLabDevices(GetLabDevicesRequestDto req)
        {
            var token = Request.Cookies["token"];
            var role = _tokenService.GetRoleFromToken(token);
            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "You do not have permission to perform this action"
                });
            }

            var devices = await _labDeviceRepository.GetLabDevices(req);
            return Ok(new
            {
                Success = true,
                Devices = devices
            });
        }

        [HttpPost("add-devices-to-lab")]
        public async Task<ActionResult> AddDevicesToLab(AddDevicesToLabRequestDto req)
        {
            var token = Request.Cookies["token"];
            var role = _tokenService.GetRoleFromToken(token);
            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "You do not have permission to perform this action"
                });
            }

            var result = await _labDeviceRepository.AddDevicesToLab(req.LabId, req.DeviceIds);
            if (result)
            {
                return Ok(new
                {
                    Success = true,
                    Message = "Devices added to lab successfully"
                });
            }
            else
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to add devices to lab"
                });
            }
        }
        [HttpPost("remove-devices-from-lab")]
        public async Task<ActionResult> RemoveDevicesFromLab(RemoveDevicesFromLabRequestDto req)
        {
            var token = Request.Cookies["token"];
            var role = _tokenService.GetRoleFromToken(token);
            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "You do not have permission to perform this action"
                });
            }

            var result = await _labDeviceRepository.RemoveDevicesFromLab(req.DeviceIds);
            if (result)
            {
                return Ok(new
                {
                    Success = true,
                    Message = "Devices removed from lab successfully"
                });
            }
            else
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to remove devices from lab"
                });
            }
        }

    }
}
