using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Hod { get; set; } // User ID

        public string Description { get; set; }
    }
}
