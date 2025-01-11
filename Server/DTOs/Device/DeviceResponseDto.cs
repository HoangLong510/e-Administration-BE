﻿namespace Server.DTOs.Device
{
    public class DeviceResponseDto
    {
        public int Id { get; set; }
        public int? LabId { get; set; }
        public string? LabName { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; } 
        public bool Status { get; set; }
    }
}
