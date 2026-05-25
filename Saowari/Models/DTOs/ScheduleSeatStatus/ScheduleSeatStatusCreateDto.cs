namespace Saowari.Models.DTOs.ScheduleSeatStatus
{
    public class ScheduleSeatStatusCreateDto
    {
        public int StatusID { get; set; }
        public int ScheduleID { get; set; }
        public int SeatID { get; set; }
        public int? BookingID { get; set; }
        public int SeatStatusId { get; set; }
    }
}