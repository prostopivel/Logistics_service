using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics_service.Models.Users
{
    public class Customer : User
    {
        public List<int>? OrdersId { get; set; }

        public Customer() { }

        public Customer(User user)
        {
            if (user.Role != UserRole.Customer)
            {
                throw new Exception("Пользователь не заказчик!");
            }

            Name = user.Name;
            Role = user.Role;
            Email = user.Email;
            PasswordHash = user.PasswordHash;
        }
    }
}
