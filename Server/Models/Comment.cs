namespace Server.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public Report? Report { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string Content { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    }
}
