namespace Saowari.Models.DTOs.Seat
{
    public class SeatResponseDto
    {
        public int SeatID { get; set; }
        public int VehicleId { get; set; }
        public string SeatNumber { get; set; }
        public decimal SeatPriceing { get; set; }
        public int SeatClassId { get; set; }
        public bool IsActive { get; set; }
    }
}