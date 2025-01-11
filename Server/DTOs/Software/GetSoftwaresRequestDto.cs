namespace Server.DTOs.Software
{
    public class GetSoftwaresRequestDto
    {
        public string? SearchValue { get; set; }
        public bool? Status { get; set; }
        public int PageNumber { get; set; }
    }
}
