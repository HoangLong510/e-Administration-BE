using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class Document
    {
        public int Id { get; set; }
        public int? LabId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }

    }
}