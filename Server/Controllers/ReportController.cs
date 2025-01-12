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
        private readonly INotificationRepository _notificationRepo;
        private readonly ITaskRepository taskRepo;
        public ReportController(ITaskRepository taskRepo, INotificationRepository notificationRepo, IReportRepository reportRepo, IUserRepository userRepo, TokenService tokenService , ICommentRepository commentRepo)
        {
            _reportRepo = reportRepo;
            _userRepo = userRepo;
            _tokenService = tokenService;
            _commentRepo = commentRepo;
            _notificationRepo = notificationRepo;
            this.taskRepo = taskRepo;
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
            var admins = await _userRepo.GetUsersByRoleAsync(UserRole.Admin);

            foreach (var admin in admins)
            {
                var notification = new Notification
                {
                    SenderId = senderId,
                    ReceiverId = admin.Id,
                    Content = $"A new report has been created: {Enum.GetName(typeof(ReportTitle), report.Title)}",
                    ReportId = createdReport.Id,
                    ActionType = "NewReport",
                    CreatedAt = DateTime.Now
                };

                await _notificationRepo.CreateNotiReportAsync(notification);
            }

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
                var tasks = await taskRepo.GetTaskByReportId(id);
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
                        }),
                        Tasks = tasks.Select(t => new
                        {
                            t.Id,
                            t.Title,
                            t.Content,
                            t.AssigneesId,
                            AssigneeFullName = t.Assignees?.FullName,
                            t.CreatedAt,
                            t.ComplatedAt,
                            t.Status
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

            if (sender.Role == UserRole.Admin)
            {
                var notification = new Notification
                {
                    SenderId = sender.Id,
                    ReceiverId = report.SenderId,
                    Content = $"{sender.FullName} commented on your report: {comment.Content}",
                    ReportId = reportId,
                    ActionType = "AdminComment",
                    CreatedAt = DateTime.Now
                };

                await _notificationRepo.CreateNotiReportAsync(notification);
            }
            else
            {
                var admins = await _userRepo.GetUsersByRoleAsync(UserRole.Admin);

                foreach (var admin in admins)
                {
                    var notification = new Notification
                    {
                        SenderId = sender.Id,
                        ReceiverId = admin.Id,
                        Content = $"{sender.FullName} added a comment on their own report: {comment.Content}",
                        ReportId = reportId,
                        ActionType = "UserComment",
                        CreatedAt = DateTime.Now
                    };

                    await _notificationRepo.CreateNotiReportAsync(notification);
                }
            }

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
            var token = Request.Cookies["token"];

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { Success = false, Message = "No token provided." });
            }

            var userId = _tokenService.GetUserIdFromToken(token);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Success = false, Message = "Invalid token." });
            }

            var sender = await _userRepo.GetUserById(int.Parse(userId));
            if (sender == null)
            {
                return Unauthorized(new { Success = false, Message = "User not found." });
            }

            if (!Enum.IsDefined(typeof(ReportStatus), status))
            {
                return BadRequest(new { Success = false, Message = "Invalid status value" });
            }

            var report = await _reportRepo.GetReportByIdAsync(id);
            if (report == null)
            {
                return NotFound(new { Success = false, Message = "Report not found" });
            }

            if (report.Status == status)
            {
                return Ok(new
                {
                    Success = true,
                    Message = "The report is already in the requested status."
                });
            }

            var updatedReport = await _reportRepo.UpdateReportStatusAsync(id, status);

            var notification = new Notification
            {
                SenderId = sender.Id,
                ReceiverId = report.SenderId,
                Content = $"Your report status has been changed to: {Enum.GetName(typeof(ReportStatus), status)}",
                ReportId = id,
                ActionType = "StatusChange",
                CreatedAt = DateTime.Now
            };

            await _notificationRepo.CreateNotiReportAsync(notification);

            if (updatedReport == null)
            {
                return NotFound(new { Success = false, Message = "Report not found" });
            }

            return Ok(new
            {
                Success = true,
                Message = "Report status updated and notification sent.",
                Data = new
                {
                    updatedReport.Id,
                    updatedReport.Title,
                    updatedReport.Content,
                    Status = Enum.GetName(typeof(ReportStatus), updatedReport.Status),
                    updatedReport.SenderId,
                    updatedReport.LastUpdated
                }
            });
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
