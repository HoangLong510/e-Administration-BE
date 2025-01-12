using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Hod { get; set; } // User ID

        [NotMapped]
        public User? User { get; set; }

        public string Description { get; set; }

        public ICollection<User>? Users { get; set; }
    }
}
