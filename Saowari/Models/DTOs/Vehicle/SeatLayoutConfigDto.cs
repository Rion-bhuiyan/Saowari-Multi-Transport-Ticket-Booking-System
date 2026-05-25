namespace Saowari.Models.DTOs.Vehicle
{
    public class SeatLayoutConfigDto
    {
        public bool IsDoubleDecker { get; set; }
        public bool ContinuousBackRow { get; set; }
        public string? LayoutPreset { get; set; }
    }

    public class SeatClassAssignmentDto
    {
        public string SeatNumber { get; set; } = null!;
        public int SeatClassId { get; set; }
    }
}
