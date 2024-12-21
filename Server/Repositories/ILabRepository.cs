using Server.Models;

namespace Server.Repositories
{
    public interface ILabRepository
    {
        Task<IEnumerable<Lab>> GetAllLabsAsync(string? searchQuery, string? statusFilter);
        Task<Lab> GetLabByIdAsync(int id);
        Task<Lab> CreateLabAsync(Lab lab);
        Task<Lab> UpdateLabAsync(int id, Lab lab);
        Task<bool> DeleteLabAsync(int id);
    }
}
