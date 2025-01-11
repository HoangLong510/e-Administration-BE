namespace Server.DTOs.LabDevice
{
    public class GetLabDevicesRequestDto
    {
        public string? SearchValue { get; set; }
        public int? LabId { get; set; }
    }
}
