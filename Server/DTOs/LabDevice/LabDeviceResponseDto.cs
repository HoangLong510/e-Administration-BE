namespace Server.DTOs.LabDevice
{
    public class LabDeviceResponseDto
    {
        public int Id { get; set; } 
        public string Name { get; set; } 
        public string Type { get; set; } 
        public string? Description { get; set; } 
        public string? LicenseExpire { get; set; } 
        public bool IsSoftware { get; set; }
    }
}
