using System.ComponentModel.DataAnnotations;

namespace Logistics_service.Models
{
    public class Map
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Data is required")]
        public string Data { get; set; }

        public void Update()
        {
            // Логика обновления карты в реальном времени
        }

        public void Display()
        {
            // Логика отображения карты с маршрутами машин
        }
    }
}
