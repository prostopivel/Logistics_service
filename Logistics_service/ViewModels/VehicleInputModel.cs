using System.ComponentModel.DataAnnotations;

namespace Logistics_service.ViewModels
{
    public class VehicleInputModel
    {
        [Required(ErrorMessage = "Speed is required")]
        public int Speed { get; set; }


        [Required(ErrorMessage = "Garage is required")]
        public int GarageId { get; set; }
    }
}
