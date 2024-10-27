using System.ComponentModel.DataAnnotations;

namespace Logistics_service.Models
{
    public class Customer : User
    {
        public List<Order>? Orders { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot be longer than 200 characters")]
        public string Address { get; set; }

        public void ViewOrders()
        {
            // Логика просмотра заказов
        }

        public void CreateRequest()
        {
            // Логика создания запроса на доставку
        }

        public void ViewAssignedVehicles()
        {
            // Логика просмотра назначенных машин
        }
    }
}
