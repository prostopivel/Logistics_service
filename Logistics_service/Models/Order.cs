using System.ComponentModel.DataAnnotations;

namespace Logistics_service.Models
{
    public class Order
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Vehicle is required")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        public int CustomerId { get; set; }

        public Customer? Customer { get; set; }

        public Vehicle? Vehicle { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot be longer than 50 characters")]
        public string Status { get; set; }

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
