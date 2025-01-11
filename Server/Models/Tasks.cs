using Server.Models.Enums;
using System.Text.Json.Serialization;

namespace Server.Models
{
    public class Tasks
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public int? AssigneesId { get; set; }

        public User? Assignees { get; set; }

        public int? ReportId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ComplatedAt { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TaskStatusEnum Status { get; set; } = TaskStatusEnum.Pending;
    }
}
