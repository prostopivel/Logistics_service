using System.ComponentModel.DataAnnotations;

namespace Logistics_service.ViewModels.OrderModels
{
    public class ManagerOrderInputModel
    {
        public int? Id { get; set; }

        public string? CustomerEmail { get; set; }

        [Required(ErrorMessage = "StartPointId is required")]
        [Range(0, int.MaxValue, ErrorMessage = "StartPointId must be a non-negative number.")]
        public int StartPointId { get; set; }

        [Required(ErrorMessage = "EndPointId is required")]
        [Range(0, int.MaxValue, ErrorMessage = "EndPointId must be a non-negative number.")]
        public int EndPointId { get; set; }

        [Required(ErrorMessage = "VehicleId is required")]
        [Range(0, int.MaxValue, ErrorMessage = "VehicleId must be a non-negative number.")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "ArrivalTime is required")]
        public DateTime ArrivalTime { get; set; }
    }
}