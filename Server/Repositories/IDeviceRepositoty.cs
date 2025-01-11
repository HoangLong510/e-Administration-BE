using System.Threading.Tasks;
using Server.DTOs.Device;
using Server.Models;

namespace Server.Repositories
{
    public interface IDeviceRepository
    {
        Task<(List<DeviceResponseDto> devices, int totalPages)> GetDevices(GetDevicesRequestDto req);
        Task<bool> CreateDevice(DeviceCreateDto device);
        Task<(bool success, string message)> DisableDevice(int deviceId);
        Task<bool> UpdateDevice(int deviceId, DeviceUpdateDto request);

        Task<Device> GetDeviceById(int deviceId);


    }
}
