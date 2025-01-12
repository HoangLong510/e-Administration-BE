
namespace Server.DTOs.Document
{
    public class DocumentResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
        public bool Status { get; set; }
    }
}
