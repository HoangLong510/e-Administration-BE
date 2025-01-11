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
        private readonly ICommentRepository _commentRepo;
        public ReportController(IReportRepository reportRepo, IUserRepository userRepo, TokenService tokenService , ICommentRepository commentRepo)
        {
            _reportRepo = reportRepo;
            _userRepo = userRepo;
            _tokenService = tokenService;
            _commentRepo = commentRepo;
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
            try
            {
                var report = await _reportRepo.GetReportByIdAsync(id);
                if (report == null)
                    return NotFound(new { Success = false, Message = "Report not found" });

                var comments = await _commentRepo.GetCommentsByReportIdAsync(id);
                return Ok(new
                {
                    Success = true,
                    Data = new
                    {
                        report.Id,
                        report.Title,
                        report.Content,
                        report.Status,
                        report.Images,
                        report.SenderId,
                        SenderFullName = report.Sender?.FullName,
                        report.CreationTime,
                        report.LastUpdated,
                        Comments = comments.Select(c => new
                        {
                            c.Id,
                            c.UserId,
                            UserFullName = c.User?.FullName,
                            c.Content,
                            c.CreationTime
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred while processing your request" });
            }
        }


        [HttpGet("list")]
        public async Task<IActionResult> GetReportsByUserId(
            [FromQuery] int senderId,
            [FromQuery] ReportTitle? category = null,
            [FromQuery] ReportStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var reports = await _reportRepo.GetReportsBySenderIdAsync(senderId, category, status, page, pageSize);
            var totalCount = await _reportRepo.GetReportsCountBySenderAsync(senderId, category, status);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

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
                Data = response,
                TotalCount = totalCount,
                TotalPages = totalPages
            });
        }

        [HttpPost("{reportId}/comment")]
        public async Task<IActionResult> CreateComment(int reportId, [FromBody] Comment comment)
        {
            if (comment == null)
                return BadRequest(new { Success = false, Message = "Invalid comment data" });

            if (comment.UserId == 0)
                return BadRequest(new { Success = false, Message = "UserId is required" });

            if (reportId == 0)
                return BadRequest(new { Success = false, Message = "ReportId is required" });

            var sender = await _userRepo.GetUserById(comment.UserId);
            if (sender == null)
                return NotFound(new { Success = false, Message = "Sender not found" });

            var report = await _reportRepo.GetReportByIdAsync(reportId);
            if (report == null)
                return NotFound(new { Success = false, Message = "Report not found" });

            comment.ReportId = reportId;
            comment.UserId = sender.Id;

            var createdComment = await _commentRepo.CreateCommentAsync(comment);

            return Ok(new
            {
                Success = true,
                Message = "Comment created successfully",
                Data = new
                {
                    createdComment.Id,
                    createdComment.UserId,
                    createdComment.ReportId,
                    UserFullName = createdComment.User?.FullName,
                    createdComment.Content,
                    createdComment.CreationTime
                }
            });
        }


        [HttpGet("all")]
        public async Task<IActionResult> GetAllReports(
            [FromQuery] ReportTitle? category = null,
            [FromQuery] ReportStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
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

            var reports = await _reportRepo.GetAllReportsAsync(category, status, page, pageSize);
            var totalCount = await _reportRepo.GetReportsCountAsync(category, status);
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
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
                Data = response,
                TotalCount = totalCount,
                TotalPages = totalPages
            });
        }


        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateReportStatus(int id, [FromBody] ReportStatus status)
        {
            if (!Enum.IsDefined(typeof(ReportStatus), status))
            {
                return BadRequest(new { Success = false, Message = "Invalid status value" });
            }

            var updatedReport = await _reportRepo.UpdateReportStatusAsync(id, status);
            if (updatedReport == null)
            {
                return NotFound(new { Success = false, Message = "Report not found" });
            }

            return Ok(new { Success = true, Data = updatedReport });
        }


        [HttpGet("totalreportPending")]
        public async Task<IActionResult> GetTotalPendingReports()
        {
            var totalPending = await _reportRepo.GetTotalPendingReportsAsync();
            return Ok(new { Success = true, TotalPending = totalPending });
        }


        [HttpGet("monthly")]
        public async Task<IActionResult> GetReportsByMonth([FromQuery] int year)
        {
            if (year <= 0)
            {
                return BadRequest(new { Success = false, Message = "Invalid year" });
            }

            var reportsByMonth = await _reportRepo.GetReportsCountByYearAsync(year);

            return Ok(new
            {
                Success = true,
                Message = "Reports retrieved successfully!",
                Data = reportsByMonth
            });
        }

    }
}
