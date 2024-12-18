using Server.Models.Enums;
using System.Text.Json.Serialization;

namespace Server.Models
{
    public class User
    {

        public int Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string FullName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? Avatar { get; set; }

        public DateTime? DateOfBirth { get; set; } = null;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserGender Gender { get; set; } = UserGender.Other;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserRole Role { get; set; }

        public int? ClassId { get; set; }

        public int? DepartmentId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
