namespace Server.DTOs.Schedule
{
    public class GetScheduleDto
    {
        public int Id { get; set; }
        public string Course { get; set; }
        public string Lab { get; set; }
        public string Class { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        
    }
}
