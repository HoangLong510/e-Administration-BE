namespace Server.Models
{
    public class Software
    {
        public int Id { get; set; }
        public int? LabId { get; set; }  
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime? LicenseExpire { get; set; }
        public bool Status { get; set; }
        public Lab? Lab { get; set; }
    }
}
