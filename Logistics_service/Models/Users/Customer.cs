using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics_service.Models.Users
{
    public class Customer : User
    {
        public List<int>? OrdersId { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot be longer than 200 characters")]
        [NotMapped]
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

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
