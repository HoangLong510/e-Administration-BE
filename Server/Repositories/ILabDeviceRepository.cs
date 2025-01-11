using Server.DTOs.LabDevice;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Repositories
{
    public interface ILabDeviceRepository
    {
        Task<List<LabDeviceResponseDto>> GetLabDevices(GetLabDevicesRequestDto req);
        Task<List<LabDeviceResponseDto>> GetAddToLab(GetLabDevicesRequestDto req);
        Task<bool> AddDevicesToLab(int labId, List<int> deviceIds);
        Task<bool> RemoveDevicesFromLab(List<int> deviceIds); 
    }
}
