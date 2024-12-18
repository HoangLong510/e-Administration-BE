namespace Server.DTOs.User
{
    public class GetUsersRequestDto
    {
        public bool IsActive { get; set; }
        public string? Role { get; set; }
        public int PageNumber { get; set; }
        public string? SearchValue { get; set; }
    }
}
