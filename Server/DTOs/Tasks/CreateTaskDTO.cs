namespace Server.DTOs.Tasks
{
    public class CreateTaskDTO
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public int AssigneesId { get; set; }

        public int? ReportId { get; set; }
    }
}
