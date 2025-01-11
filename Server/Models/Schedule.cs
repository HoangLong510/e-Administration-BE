using Server.Models.Enums;
using System.Text.Json.Serialization;

namespace Server.Models
{
    public class Schedule
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Course { get; set; }
        public string Lab { get; set; }
        public string Class { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
