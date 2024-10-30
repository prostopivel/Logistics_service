using System.ComponentModel.DataAnnotations;

namespace Logistics_service.Models
{
    public class Order
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Vehicle is required")]
        public int VehicleId { get; set; }

        public int? CustomerId { get; set; }

        public string? Status { get; set; }

        [Required(ErrorMessage = "Delivery date is required")]
        public DateTime DeliveryDate { get; set; }

        public DateTime? CreatedAt { get; set; }

        public List<Point>? Points { get; set; }

        public void UpdateStatus(string newStatus)
        {
            // Логика обновления статуса заказа
        }
    }
}
