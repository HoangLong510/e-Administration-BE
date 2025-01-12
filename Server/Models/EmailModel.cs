using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class EmailModel
    {
        [Key]
        public int Id { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public string Status { get; set; }

        public DateTime? SentDate { get; set; }
    }
}
