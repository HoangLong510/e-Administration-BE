namespace Server.DTOs.Tasks
{
    public class GetAllTasksRequest
    {
        public string? Status { get; set; }
        public int PageNumber { get; set; }
        public string? SearchValue { get; set; }
        public int? UserId { get; set; }
    }
}
