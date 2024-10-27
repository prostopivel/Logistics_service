using System.ComponentModel.DataAnnotations;

namespace Logistics_service.Models
{
    public class Point
    {
        public int? Id { get; set; }


        [Required(ErrorMessage = "Route is required")]
        public int RouteId { get; set; }

        public Route? Route { get; set; }

        [Required(ErrorMessage = "Order is required")]
        public int OrderId { get; set; }

        public Order? Order { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(100, ErrorMessage = "Location cannot be longer than 100 characters")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Sequence number is required")]
        public int SequenceNumber { get; set; }


        public void UpdateLocation(string newLocation)
        {
            // Логика обновления местоположения точки
        }
    }
}
