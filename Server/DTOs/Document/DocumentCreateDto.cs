namespace Server.DTOs.Document
{
    public class DocumentCreateDto
    {
        public string Name { get; set; }
        public IFormFile File { get; set; }
        public bool Status { get; set; } = true; 
    }
}
