using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.DTOs.Schedule;
using Server.Models;
using Server.Repositories;
using Server.Services;
using System.Globalization;
using System.Linq;
using System.Web;
using OfficeOpenXml;
using Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleRepository scheduleRepository;

        private readonly TokenService tokenService;
        private readonly DatabaseContext db;

        public ScheduleController(IScheduleRepository scheduleRepository, TokenService tokenService, DatabaseContext db)
        {
            this.scheduleRepository = scheduleRepository;
            this.tokenService = tokenService;
            this.db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetSchedules([FromQuery] bool includeAll = true)
        {
            var token = Request.Cookies["token"];
            var userIdString = tokenService.GetUserIdFromToken(token);
            var userRole = tokenService.GetRoleFromToken(token);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid token.");
            }

            if (userRole == "Admin" || userRole == "Student" || userRole == "Instructor")
            {
                if (userRole == "Student")
                {

                    var User = await scheduleRepository.GetUserByUserIdAsync(userId);
                    if (User == null || User.ClassId.Value == null)
                    {
                        return NotFound("User is not assigned to any class.");
                    }

                    var userClass = await scheduleRepository.GetClassByIDAsync(User.ClassId.Value);
                    if (userClass == null)
                    {
                        return NotFound("Class not found.");
                    }

                    var allSchedules = await scheduleRepository.GetAllSchedulesAsync();
                    var query = from schedule in db.Schedules
                                join Lab in db.Labs on schedule.Lab equals Lab.Name
                                where Lab.Status == true
                                select schedule;
                    var studentSchedules = query
                        .Where(s => s.Class == userClass.Name)
                        .OrderBy(s => s.StartTime)
                        .ToList();

                    return Ok(studentSchedules);
                }
                else if (userRole == "Instructor")
                {
                    var Schedules = await scheduleRepository.GetSchedulesByUserIdAsync(userId);
                    if (Schedules == null || !Schedules.Any())
                    {
                        return NotFound("No schedules found for this Instructor.");
                    }
                    var query = from schedule in db.Schedules
                                join Lab in db.Labs on schedule.Lab equals Lab.Name
                                where Lab.Status == true
                                select schedule;

                    var sortedSchedules = query
                                        .OrderBy(s => s.StartTime)
                                        .ToList();
                    return Ok(sortedSchedules);
                }
                else
                {
                    var allSchedules = await scheduleRepository.GetAllSchedulesAsync();
                    var sortedSchedules = allSchedules.OrderBy(s => s.StartTime).ToList();
                    return Ok(sortedSchedules);
                }
            }
            else
            {
                return Unauthorized("Unauthorized access.");
            }
        }

        [HttpGet("schedule/{id}")]
        public async Task<IActionResult> GetScheduleWithUserFullName(int id)
        {
            var token = Request.Cookies["token"];
            var userIdString = tokenService.GetUserIdFromToken(token);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid token or userId.");
            }

            var allSchedules = await scheduleRepository.GetAllSchedulesAsync();

            var schedule = allSchedules.OrderBy(s => s.StartTime).FirstOrDefault(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound($"Schedule with Id {id} not found.");
            }

            var fullName = await scheduleRepository.GetFullNameByUserIdAsync(schedule.UserId);

            var scheduleDto = new GetScheduleDto
            {
                Id = schedule.Id,
                Course = schedule.Course,
                Lab = schedule.Lab,
                Class = schedule.Class,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                UserId = schedule.UserId,
                FullName = fullName
            };

            return Ok(scheduleDto);
        }

        [HttpGet("lab/{lab}")]
        public async Task<IActionResult> GetSchedulesByLab(string lab)
        {
            var token = Request.Cookies["token"];
            var userIdString = tokenService.GetUserIdFromToken(token);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid token.");
            }

            var schedules = await scheduleRepository.GetSchedulesByLabAsync(lab);

            if (schedules == null || !schedules.Any())
            {
                return NotFound($"No schedules found for lab containing '{lab}'.");
            }

            return Ok(schedules);
        }

        [HttpGet("week/{week}")]
        public async Task<IActionResult> GetSchedulesByWeek(string week)
        {
            var token = Request.Cookies["token"];
            var userIdString = tokenService.GetUserIdFromToken(token);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid token.");
            }

            var decodedWeek = HttpUtility.UrlDecode(week);

            var weekDates = decodedWeek.Split('-');
            if (weekDates.Length != 2)
            {
                return BadRequest("Invalid week format. Please use 'dd/MM-dd/MM' format.");
            }

            var startDate = DateTime.ParseExact(weekDates[0], "dd/MM", CultureInfo.InvariantCulture);
            var endDate = DateTime.ParseExact(weekDates[1], "dd/MM", CultureInfo.InvariantCulture);

            var schedules = await scheduleRepository.GetSchedulesByUserIdAsync(userId);
            var filteredSchedules = schedules
                .Where(s => s.StartTime.Date >= startDate.Date && s.StartTime.Date <= endDate.Date)
                .OrderBy(s => s.StartTime)
                .ToList();

            return Ok(filteredSchedules);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleDto scheduleDto)
        {
            var token = Request.Cookies["token"];
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is missing.");
            }

            var userIdString = tokenService.GetUserIdFromToken(token);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid token.");
            }

            var existingSchedules = await scheduleRepository.GetSchedulesByLabAsync(scheduleDto.Lab);

            if (existingSchedules != null && existingSchedules.Any())
            {
                foreach (var existingSchedule in existingSchedules)
                {
                    if ((scheduleDto.StartTime < existingSchedule.EndTime && scheduleDto.StartTime >= existingSchedule.StartTime) ||
                        (scheduleDto.EndTime > existingSchedule.StartTime && scheduleDto.EndTime <= existingSchedule.EndTime) ||
                        (scheduleDto.StartTime <= existingSchedule.StartTime && scheduleDto.EndTime >= existingSchedule.EndTime))
                    {
                        return Conflict(new { message = "Time conflict detected. Please choose a different time slot." });
                    }
                }
            }

            var schedule = new Schedule
            {
                Course = scheduleDto.Course,
                Lab = scheduleDto.Lab,
                Class = scheduleDto.Class,
                StartTime = scheduleDto.StartTime,
                EndTime = scheduleDto.EndTime,
                UserId = userId
            };

            await scheduleRepository.CreateScheduleAsync(schedule);


            return CreatedAtAction(nameof(GetScheduleWithUserFullName), new { id = schedule.Id },
                new { message = "Schedule created successfully.", schedule });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var token = Request.Cookies["token"];
            var userIdString = tokenService.GetUserIdFromToken(token);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid token.");
            }

            var existingSchedule = await scheduleRepository.GetScheduleByIdAsync(id);
            if (existingSchedule == null)
            {
                return NotFound($"Schedule with Id {id} not found.");
            }

            await scheduleRepository.DeleteScheduleAsync(id);

            return new JsonResult(new { success = true, message = "Schedule deleted successfully!" });
        }

        [HttpGet("fullname/{fullName}")]
        public async Task<IActionResult> GetScheduleByFullName(string fullName)
        {
            var schedules = await scheduleRepository.GetSchedulesByFullNameAsync(fullName);

            if (!schedules.Any())
            {
                return NotFound($"No schedules found for user {fullName}.");
            }

            var scheduleDtos = schedules.Select(s => new GetScheduleDto
            {
                Id = s.Id,
                Course = s.Course,
                Lab = s.Lab,
                Class = s.Class,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                UserId = s.UserId,
                FullName = fullName
            }).ToList();

            return Ok(scheduleDtos);
        }

        [HttpGet("export/{userId}")]
        public async Task<IActionResult> ExportSchedulesToExcelAsync(int userId)
        {
            var schedules = await scheduleRepository.GetSchedulesByUserIdAsync(userId);
            if (schedules == null || !schedules.Any())
            {
                return NotFound($"No schedules found for user with UserId: {userId}.");
            }

            var userIds = schedules.Select(s => s.UserId).Distinct();
            var userFullNames = new Dictionary<int, string>();
            foreach (var id in userIds)
            {
                var fullName = await scheduleRepository.GetFullNameByUserIdAsync(id);
                userFullNames[id] = fullName;
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Schedules");

                worksheet.Cells[1, 1, 1, 7].Merge = true;
                worksheet.Cells[1, 1].Value = "Schedule";
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 18;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Cells[1, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

                worksheet.Cells[2, 1].Value = "ID";
                worksheet.Cells[2, 2].Value = "Course";
                worksheet.Cells[2, 3].Value = "Lab";
                worksheet.Cells[2, 4].Value = "Class";
                worksheet.Cells[2, 5].Value = "Date";
                worksheet.Cells[2, 6].Value = "Time";
                worksheet.Cells[2, 7].Value = "Lecturer";

                worksheet.Cells[2, 1, 2, 7].Style.Font.Bold = true;
                worksheet.Cells[2, 1, 2, 7].Style.Font.Size = 14;
                worksheet.Cells[2, 1, 2, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[2, 1, 2, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                worksheet.Cells[2, 1, 2, 7].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                worksheet.Cells[2, 1, 2, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[2, 1, 2, 7].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                worksheet.Column(1).Width = 7;
                worksheet.Column(2).Width = 25;
                worksheet.Column(3).Width = 12;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Width = 18;
                worksheet.Column(6).Width = 25;
                worksheet.Column(7).Width = 30;

                int row = 3;
                int idCounter = 1;
                foreach (var schedule in schedules)
                {
                    var fullName = userFullNames.GetValueOrDefault(schedule.UserId, "Unknown");

                    var date = schedule.StartTime.ToString("yyyy-MM-dd");
                    var startTime = schedule.StartTime.ToString("hh:mm tt");
                    var endTime = schedule.EndTime.ToString("hh:mm tt");

                    worksheet.Cells[row, 1].Value = idCounter++;
                    worksheet.Cells[row, 2].Value = schedule.Course;
                    worksheet.Cells[row, 3].Value = schedule.Lab;
                    worksheet.Cells[row, 4].Value = schedule.Class;
                    worksheet.Cells[row, 5].Value = date;
                    worksheet.Cells[row, 6].Value = $"{startTime} - {endTime}";
                    worksheet.Cells[row, 7].Value = fullName;

                    worksheet.Cells[row, 1, row, 7].Style.Font.Size = 14;
                    worksheet.Cells[row, 1, row, 7].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin, System.Drawing.Color.Black);

                    worksheet.Cells[row, 1, row, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 1, row, 7].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    if (row % 2 == 0)
                    {
                        worksheet.Cells[row, 1, row, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        worksheet.Cells[row, 1, row, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
                    }

                    row++;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return new FileContentResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"Schedules_{userId}.xlsx"
                };
            }
        }

        [HttpGet("allclass")]
        public async Task<IActionResult> GetAllClass()
        {
            var token = Request.Cookies["token"];
            var userIdString = tokenService.GetUserIdFromToken(token);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid token.");
            }


            var allClasses = await scheduleRepository.GetAllClassAsync();

            return Ok(new
            {
                Success = true,
                Message = "Get class list successfully.",
                Data = allClasses
            });
        }

        [HttpGet("alllab")]
        public async Task<IActionResult> GetAllLab()
        {
            var token = Request.Cookies["token"];
            var userIdString = tokenService.GetUserIdFromToken(token);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid token.");
            }


            var allLab = await scheduleRepository.GetAllLabAsync();
            var availableLab = allLab.Where(s => s.Status == true).ToList();
            return Ok(new
            {
                Success = true,
                Message = "Get Lab list successfully.",
                Data = availableLab
            });
        }
        [HttpGet("GetScheduleByCondition")]
        public async Task<IActionResult> GetScheduleByConditionAsync([FromQuery] string Name, [FromQuery] string Lab)
        {
            var query = from schedule in db.Schedules
                        join user in db.Users on schedule.UserId equals user.Id
                        select new { schedule, user };

            if (!string.IsNullOrEmpty(Name))
            {

                query = query.Where(x => x.user.FullName.Contains(Name));
            }
            if (!string.IsNullOrEmpty(Lab))
            {
                query = query.Where(x => x.schedule.Lab.Contains(Lab));
            }

            var result = await query.Select(x => new GetScheduleDto
            {
                Id = x.schedule.Id,
                Course = x.schedule.Course,
                Lab = x.schedule.Lab,
                Class = x.schedule.Class,
                StartTime = x.schedule.StartTime,
                EndTime = x.schedule.EndTime,
                UserId = x.schedule.UserId,
                FullName = x.user.FullName
            }).ToListAsync();


            if (result.Any())
            {
                return Ok(result);
            }
            else
            {
                return NotFound("No suitable schedule found!");
            }
        }
    }
}

