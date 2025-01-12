using Server.Models.Enums;
using System;
using System.Collections.Generic;

namespace Server.Models
{
    public class Report
    {
        public int Id { get; set; }
        public ReportTitle Title { get; set; }
        public string? Content { get; set; }
        public ReportStatus Status { get; set; } = ReportStatus.Pending;
        public List<string>? Images { get; set; }
        public int SenderId { get; set; }
        public User Sender { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public List<Comment> Comments { get; set; }
        public ICollection<Tasks>? Tasks { get; set; }
    }
}
