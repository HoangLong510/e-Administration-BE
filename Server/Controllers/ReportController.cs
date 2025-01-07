using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Models.Enums;
using Server.Repositories;
using Microsoft.AspNetCore.Authorization;
using Server.Services;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportRepository _reportRepo;
        private readonly IUserRepository _userRepo;
        private readonly TokenService _tokenService;

        public ReportController(IReportRepository reportRepo, IUserRepository userRepo, TokenService tokenService)
        {
            _reportRepo = reportRepo;
            _userRepo = userRepo;
            _tokenService = tokenService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport([FromForm] string title, [FromForm] string content, [FromForm] int senderId, [FromForm] List<IFormFile> images)
        {
            var sender = await _userRepo.GetUserById(senderId);
            if (sender == null)
            {
                return BadRequest(new { Success = false, Message = "Invalid SenderId" });
            }

            if (!Enum.TryParse(title, out ReportTitle reportTitle))
            {
                return BadRequest(new { Success = false, Message = "Invalid ReportTitle" });
            }

            var report = new Report
            {
                Title = reportTitle,
                Content = content,
                SenderId = senderId,
                Status = ReportStatus.Pending
            };

            var createdReport = await _reportRepo.CreateReportAsync(report, images);

            var response = new
            {
                Success = true,
                Data = new
                {
                    report.Id,
                    Title = Enum.GetName(typeof(ReportTitle), report.Title),
                    Content = report.Content,
                    Status = Enum.GetName(typeof(ReportStatus), report.Status),
                    report.Images,
                    report.SenderId,
                    Sender = report.Sender,
                    report.CreationTime,
                    report.LastUpdated
                }
            };

            return Ok(response);
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetReport(int id)
        {
            var report = await _reportRepo.GetReportByIdAsync(id);
            if (report == null)
                return NotFound(new { Success = false, Message = "Report not found" });

            return Ok(new { Success = true, Data = report });
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetReportsByUserId([FromQuery] int senderId, [FromQuery] ReportTitle? category = null)
        {
            var reports = await _reportRepo.GetReportsBySenderIdAsync(senderId, category);

            var response = reports.Select(report => new
            {
                report.Id,
                Title = Enum.GetName(typeof(ReportTitle), report.Title),
                Content = report.Content,
                Status = Enum.GetName(typeof(ReportStatus), report.Status),
                report.Images,
                report.SenderId,
                SenderFullName = report.Sender?.FullName,
                report.CreationTime,
                report.LastUpdated
            });
            return Ok(new { Success = true, Data = response });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllReports([FromQuery] ReportTitle? category = null)
        {
            var token = Request.Cookies["token"];

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "No token provided."
                });
            }

            var role = _tokenService.GetRoleFromToken(token);

            if (role != "Admin")
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "You do not have permission to view all reports."
                });
            }

            var reports = await _reportRepo.GetAllReportsAsync(category);

            var response = reports.Select(report => new
            {
                report.Id,
                Title = Enum.GetName(typeof(ReportTitle), report.Title),
                Content = report.Content,
                Status = Enum.GetName(typeof(ReportStatus), report.Status),
                report.Images,
                report.SenderId,
                SenderFullName = report.Sender?.FullName,
                report.CreationTime,
                report.LastUpdated
            });

            return Ok(new
            {
                Success = true,
                Message = "Reports retrieved successfully!",
                Data = response
            });
        }





        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateReportStatus(int id, [FromBody] ReportStatus status)
        {
            var updatedReport = await _reportRepo.UpdateReportStatusAsync(id, status);
            if (updatedReport == null)
                return NotFound(new { Success = false, Message = "Report not found" });

            return Ok(new { Success = true, Message = "Report status updated successfully", Data = updatedReport });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            await _reportRepo.DeleteReportAsync(id);
            return Ok(new { Success = true, Message = "Report deleted successfully" });
        }

    }
}
