using System.ComponentModel.DataAnnotations;

namespace Logistics_service.Models.Orders
{
    public class ManagerOrder
    {
        public string? CustomerEmail { get; set; }

        [Required(ErrorMessage = "StartPointId is required")]
        public int StartPointId { get; set; }

        [Required(ErrorMessage = "EndPointId is required")]
        public int EndPointId { get; set; }

        [Required(ErrorMessage = "VehicleId is required")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "ArrivalTime is required")]
        public DateTime ArrivalTime { get; set; }
    }
}
