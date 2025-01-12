using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class Document
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }

    }
}