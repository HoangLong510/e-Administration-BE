namespace Server.DTOs.Software
{
    public class SoftwareUpdateDto
    {
        public string Name { get; set; }
        public int? LabId { get; set; }
        public string? Type { get; set; }
        public string Description { get; set; }
        public string? LicenseExpire { get; set; }
        public bool Status { get; set; }
    }

}
