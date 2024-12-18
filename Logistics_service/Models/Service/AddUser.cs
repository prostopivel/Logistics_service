using System.ComponentModel.DataAnnotations;
using Logistics_service.Models.Users;

namespace Logistics_service.Models.Service
{
    public class AddUser
    {
        [Required(ErrorMessage = "User is required")]
        public User User { get; set; }

        [Required(ErrorMessage = "ReturnUrl is required")]
        public string ReturnUrl { get; set; }
    }
}
