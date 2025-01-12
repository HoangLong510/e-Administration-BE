namespace Server.DTOs.Document
{
    public class GetDocumentRequestDto
    {
        public string? SearchValue { get; set; }
        public bool? Status { get; set; }
        public int PageNumber { get; set; }
    }
}
