namespace Server.DTOs.User
{
    public class UpdateUserDto
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public IFormFile? Avatar { get; set; }
    }
}
