using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Lab
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public bool Status { get; set; }
    }
}
