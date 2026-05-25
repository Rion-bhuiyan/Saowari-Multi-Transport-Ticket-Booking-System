namespace Saowari.Models.DTOs.DriverInformtion
{
    public class DriverInformtionUpdateDto
    {
        public int DriverInformtionId { get; set; }
        public string LicenceNumber { get; set; }
        public DateTime licenceExpDate { get; set; }
    }
}