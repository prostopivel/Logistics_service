using Logistics_service.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Logistics_service.ViewModels
{
    public class UserInputModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserRole Role { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password hash is required")]
        [MinLength(8, ErrorMessage = "Password must be longer than 8 characters")]
        public string PasswordHash { get; set; }
    }
}
