namespace Server.DTOs.Device
{
    public class GetDevicesRequestDto
    {
        public string? SearchValue { get; set; }
        public bool? Status { get; set; } 
        public int PageNumber { get; set; }
        
    }
}
