namespace Server.DTOs.User
{
    public class UserCreateDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string? Address { get; set; }
        public string? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Role { get; set; }
        public int? ClassId { get; set; }
        public int? DepartmentId { get; set; }
    }
}
