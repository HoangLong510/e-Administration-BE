namespace Server.DTOs.LabDevice
{
    public class AddDevicesToLabRequestDto
    {
        public int LabId { get; set; }
        public List<int> DeviceIds { get; set; }
    }
}
