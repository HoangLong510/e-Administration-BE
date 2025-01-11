namespace Server.DTOs.Schedule
{
    public class CreateScheduleDto
    {
        public string Course { get; set; }
        public string Lab { get; set; }
        public string Class { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }


    }
}
