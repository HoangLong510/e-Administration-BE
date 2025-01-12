namespace Server.DTOs.Tasks
{
    public class EditTaskDto
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public int AssigneesId { get; set; }
    }
}
