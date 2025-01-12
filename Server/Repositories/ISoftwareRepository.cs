using System.Threading.Tasks;
using Server.DTOs.Software;
using Server.DTOs.Software.Server.DTOs.Software;
using Server.Models;

namespace Server.Repositories
{
    public interface ISoftwareRepository
    {
        Task<(List<SoftwareResponseDto> softwares, int totalPages)> GetSoftwares(GetSoftwaresRequestDto req);
        Task<bool> CreateSoftware(SoftwareCreateDto software);
        Task<(bool success, string message)> DisableSoftware(int softwareId);
        Task<bool> UpdateSoftware(int softwareId, SoftwareUpdateDto request);
        Task<Software> GetSoftwareById(int softwareId);
        Task<int> CountExpiredSoftware();
        Task<bool> CheckNameExists(string name);
        Task<bool> IsSoftwareNameUnique(string name, int softwareId);
        Task UpdateStatusForExpiredLicenses();
    }
}
