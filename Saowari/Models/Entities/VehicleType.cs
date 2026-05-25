namespace Saowari.Models.Entities
{
    public class VehicleType
    {
        public int VehicleTypeId { get; set; }
        public string VehicleTypeName { get; set; }= null!;

        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<Discount> Discounts { get; set; } = new List<Discount>();
    }
}
