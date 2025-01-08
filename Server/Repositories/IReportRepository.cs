using Server.Models;
using Server.Models.Enums;


namespace Server.Repositories
{
    public interface IReportRepository
    {
        Task<Report> CreateReportAsync(Report report, List<IFormFile> images);

        Task<Report> GetReportByIdAsync(int id);
        Task<List<Report>> GetReportsBySenderIdAsync(int senderId, ReportTitle? category = null, ReportStatus? status = null, int page = 1, int pageSize = 10);
        Task<Report> UpdateReportStatusAsync(int id, ReportStatus status);

        Task<List<Report>> GetAllReportsAsync(ReportTitle? category = null, ReportStatus? status = null, int page = 1, int pageSize = 10);
        Task<int> GetReportsCountAsync(ReportTitle? category = null, ReportStatus? status = null);
        Task<int> GetReportsCountBySenderAsync(int senderId, ReportTitle? category = null, ReportStatus? status = null);
        Task<int> GetTotalPendingReportsAsync();

    }
}
