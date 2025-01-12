using Server.Models;

public interface ILabRepository
{
    Task<IEnumerable<Lab>> GetAllLabsAsync(string? searchQuery, string? statusFilter);
    Task<Lab> GetLabByIdAsync(int id);
    Task<Lab> CreateLabAsync(Lab lab);
    Task<Lab> UpdateLabAsync(int id, Lab lab);
    Task<(bool success, string message)> DisableLabAsync(int LabId);
    Task<(int ActiveCount, int InactiveCount)> GetLabsStatusSummaryAsync();
    Task<bool> CheckNameExists(string name);
    Task<bool> IsLabNameUnique(string name, int labId);
}
