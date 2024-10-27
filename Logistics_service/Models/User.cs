using System.ComponentModel.DataAnnotations;

namespace Logistics_service.Models
{
    public class User
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [StringLength(50, ErrorMessage = "Role cannot be longer than 50 characters")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password hash is required")]
        [MinLength(8, ErrorMessage = "Password must be longer than 8 characters")]
        public string PasswordHash { get; set; }

        public List<Notification>? Notifications { get; set; }

        public void Login()
        {
            // Логика входа в систему
        }

        public void Logout()
        {
            // Логика выхода из системы
        }
    }
}
