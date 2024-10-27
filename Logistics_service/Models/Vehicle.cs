using System.ComponentModel.DataAnnotations;

namespace Logistics_service.Models
{
    public class Vehicle
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Current location is required")]
        [StringLength(100, ErrorMessage = "Current location cannot be longer than 100 characters")]
        public string CurrentLocation { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot be longer than 50 characters")]
        public string Status { get; set; }

        [Required(ErrorMessage = "License plate is required")]
        [StringLength(20, ErrorMessage = "License plate cannot be longer than 20 characters")]
        public string LicensePlate { get; set; }

        public Route? Route { get; set; }

        public List<Order>? Orders { get; set; }

        public void UpdateLocation(string newLocation)
        {
            // Логика обновления текущего местоположения
        }

        public void AssignToOrder(Order order)
        {
            // Логика назначения машины на заказ
        }
    }
}
