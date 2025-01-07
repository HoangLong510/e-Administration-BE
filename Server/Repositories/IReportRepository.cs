using Server.Models;
using Server.Models.Enums;


namespace Server.Repositories
{
    public interface IReportRepository
    {
        Task<Report> CreateReportAsync(Report report, List<IFormFile> images);

        Task<Report> GetReportByIdAsync(int id);
        Task<List<Report>> GetReportsBySenderIdAsync(int senderId , ReportTitle? category = null);
        Task<List<Report>> GetReportsByStatusAsync(ReportStatus status);
        Task<Report> UpdateReportStatusAsync(int id, ReportStatus status);
        Task DeleteReportAsync(int id);

        Task<List<Report>> GetAllReportsAsync(ReportTitle? category = null);
    }
}
