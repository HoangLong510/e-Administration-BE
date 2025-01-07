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

        public async Task<List<Report>> GetAllReportsAsync(ReportTitle? category = null)
        {
            var query = _context.Reports.AsQueryable();

            if (category.HasValue)
            {
                query = query.Where(r => r.Title == category.Value);
            }

            return await query.Include(r => r.Sender).ToListAsync();
        }

        public async Task<List<Report>> GetReportsBySenderIdAsync(int senderId , ReportTitle? category = null)
        {
            var query = _context.Reports.Where(r => r.SenderId == senderId);

            if (category.HasValue)
            {
                query = query.Where(r => r.Title == category.Value);
            }

            return await query.Include(r => r.Sender).ToListAsync();
        }


        public async Task<List<Report>> GetReportsByStatusAsync(ReportStatus status)
        {
            return await _context.Reports
                .Where(r => r.Status == status)
                .ToListAsync();
        }

        public async Task<Report> UpdateReportStatusAsync(int id, ReportStatus status)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                report.Status = status;
                report.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return report;
        }

        public async Task DeleteReportAsync(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();
            }
        }
    }
}
