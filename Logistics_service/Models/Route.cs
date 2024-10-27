using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace Logistics_service.Models
{
    public class Route
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Vehicle is required")]
        public int VehicleId { get; set; }

        public Vehicle? Vehicle { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        public List<Point>? Points { get; set; }

        public void AddPoint(Point point)
        {
            // Логика добавления точки на маршрут
        }

        public void RemovePoint(Point point)
        {
            // Логика удаления точки с маршрута
        }
    }
}
