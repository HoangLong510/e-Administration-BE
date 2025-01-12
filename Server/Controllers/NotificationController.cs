using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Repositories;
using Server.Services;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository notiRepo;
        private readonly TokenService tokenService;
        private readonly IUserRepository userRepo;
        private readonly IReportRepository reportRepo;

        public NotificationController(INotificationRepository notiRepo, TokenService tokenService, IUserRepository userRepo, IReportRepository reportRepo)
        {
            this.notiRepo = notiRepo;
            this.tokenService = tokenService;
            this.userRepo = userRepo;
            this.reportRepo = reportRepo;
        }


        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            // Lấy token từ Cookie hoặc Header
            var token = Request.Cookies["token"]; // Hoặc Request.Headers["Authorization"]

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { Success = false, Message = "No token provided." });
            }

            // Giải mã token để lấy userId
            var userId = tokenService.GetUserIdFromToken(token);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Success = false, Message = "Invalid token." });
            }

            // Gọi repository để lấy danh sách thông báo
            var (notifications, unreadCount) = await notiRepo.ListNotiAsync(int.Parse(userId));

            if (notifications == null || !notifications.Any())
            {
                return NotFound(new { Success = false, Message = "No notifications found." });
            }

            var notificationsWithSender = new List<object>();

            foreach (var notification in notifications)
            {
                // Truy vấn người gửi (sender) và báo cáo (report) tuần tự
                var sender = await userRepo.GetUserById(notification.SenderId);
                var report = notification.ReportId.HasValue ? await reportRepo.GetReportByIdAsync(notification.ReportId.Value) : null;

                notificationsWithSender.Add(new
                {
                    notification.Id,
                    notification.Content,
                    notification.CreatedAt,
                    notification.Viewed,
                    SenderName = sender?.FullName ?? "Unknown", // Trả về tên người gửi hoặc "Unknown" nếu không tìm thấy
                    notification.ActionType,
                    notification.ReportId,
                    notification.TaskId,
                    ReportDetails = report,
                });
            }

            return Ok(new
            {
                Success = true,
                UnreadCount = unreadCount,
                Data = notificationsWithSender
            });
        }



        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            // Lấy token từ Cookie hoặc Header để đảm bảo người dùng hợp lệ
            var token = Request.Cookies["token"]; // Hoặc Request.Headers["Authorization"]
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { Success = false, Message = "No token provided." });
            }

            var userId = tokenService.GetUserIdFromToken(token);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Success = false, Message = "Invalid token." });
            }

            // Gọi repository để đánh dấu thông báo là đã xem
            var success = await notiRepo.MarkNotificationAsReadAsync(id);
            if (!success)
            {
                return NotFound(new { Success = false, Message = "Notification not found." });
            }

            return Ok(new { Success = true, Message = "Notification marked as read." });
        }

    }
}
