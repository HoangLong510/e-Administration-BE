using Microsoft.EntityFrameworkCore;
using Server.Models;
using Server.Models.Enums;
using Server.Data;


namespace Server.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly DatabaseContext _context;

        public ReportRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<Report> CreateReportAsync(Report report, List<IFormFile> images)
        {
            if (images != null && images.Count > 0)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Uploads");

                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                var fileNames = new List<string>();
                foreach (var image in images)
                {
                    var originalFileName = Path.GetFileName(image.FileName);
                    var uniqueFileName = $"{Guid.NewGuid()}_{originalFileName}";
                    var filePath = Path.Combine(uploadDir, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    fileNames.Add(uniqueFileName);
                }

                report.Images = fileNames;
            }


            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<Report> GetReportByIdAsync(int id)
        {
            return await _context.Reports
                .Include(r => r.Sender)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        public async Task<int> GetReportsCountBySenderAsync(int senderId, ReportTitle? category = null, ReportStatus? status = null)
        {
            var query = _context.Reports.AsQueryable();

            query = query.Where(r => r.SenderId == senderId);

            if (category.HasValue)
            {
                query = query.Where(r => r.Title == category.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            return await query.CountAsync();
        }

        public async Task<List<Report>> GetAllReportsAsync(ReportTitle? category = null, ReportStatus? status = null, int page = 1, int pageSize = 10)
        {
            var query = _context.Reports.AsQueryable();

            if (category.HasValue)
            {
                query = query.Where(r => r.Title == category.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            return await query
                .Include(r => r.Sender)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }


        public async Task<List<Report>> GetReportsBySenderIdAsync(int senderId, ReportTitle? category = null, ReportStatus? status = null, int page = 1, int pageSize = 10)
        {
            var query = _context.Reports.Where(r => r.SenderId == senderId);

            if (category.HasValue)
            {
                query = query.Where(r => r.Title == category.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            return await query
                .Include(r => r.Sender)
                .Include(r => r.Comments)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetReportsCountAsync(ReportTitle? category = null, ReportStatus? status = null)
        {
            var query = _context.Reports.AsQueryable();

            if (category.HasValue)
            {
                query = query.Where(report => report.Title == category.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(report => report.Status == status.Value);
            }

            return await query.CountAsync();
        }


        public async Task<Report> UpdateReportStatusAsync(int id, ReportStatus status)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                report.Status = status;
                report.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return report;
        }


        public async Task<int> GetTotalPendingReportsAsync()
        {
            return await _context.Reports
                .CountAsync(r => r.Status == ReportStatus.Pending);
        }

        public async Task<List<int>> GetReportsCountByYearAsync(int year)
        {
            var reportsByMonth = new List<int>();

            for (int month = 1; month <= 12; month++)
            {
                var count = await _context.Reports
                    .Where(r => r.CreationTime.Year == year && r.CreationTime.Month == month)
                    .CountAsync();

                reportsByMonth.Add(count);
            }

            return reportsByMonth;
        }

    }
}
