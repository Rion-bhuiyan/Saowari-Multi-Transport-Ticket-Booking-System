namespace Saowari.Models.DTOs.Ticket
{
    public class TicketCreateDto
    {
        public int TicketID { get; set; }
        public int BookingId { get; set; }
        public string TicketCode { get; set; }
        public DateTime IssuedAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}