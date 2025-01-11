namespace Server.DTOs.Device
{
    public class DeviceCreateDto
    {
        public int? LadID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string? Image { get; set; }
        public IFormFile? ImageFile { get; set; }
        public bool Status { get; set; }
    }
}
