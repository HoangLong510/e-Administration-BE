using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public bool Viewed { get; set; } = false;

        public int SenderId { get; set; }

        public int ReceiverId { get; set; }

        public int? TaskId { get; set; }
        public int? ReportId { get; set; }

        public string ActionType { get; set; }

    }
}