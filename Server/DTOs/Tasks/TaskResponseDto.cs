using Server.DTOs.User;
using Server.Models.Enums;
using System.Text.Json.Serialization;

namespace Server.DTOs.Tasks
{
    public class TaskResponseDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public UserResponseDto? Assignees { get; set; }

        public int? ReportId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ComplatedAt { get; set; }

        public string Status { get; set; }
    }
}
