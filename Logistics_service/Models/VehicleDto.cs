namespace Logistics_service.Models
{
    public class VehicleDto
    {
        public int? Id { get; set; }
        public double? PosX { get; set; }
        public double? PosY { get; set; }

        public VehicleDto(Vehicle vehicle)
        {
            Id = vehicle.Id;
            PosX = vehicle.PosX;
            PosY = vehicle.PosY;
        }
    }
}
