using Server.Models.Enums;

namespace Server.DTOs.User
{
    public class UserResponseDto
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string FullName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? Avatar { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; }

        public string Role { get; set; }

        public int? ClassId { get; set; }

        public int? DepartmentId { get; set; }

        public bool IsActive { get; set; }
    }
}
