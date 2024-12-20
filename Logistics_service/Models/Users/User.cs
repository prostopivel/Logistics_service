using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Logistics_service.Models.Users
{
    public class User
    {
        public int? Id { get; set; }

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

        public void Login()
        {
            // Логика входа в систему
        }

        public void Logout()
        {
            // Логика выхода из системы
        }

        public override string ToString()
        {
            return string.Format($"{Id, -3}: {Name, -15} | {Email, -25} | {Role, -8}");
        }

        public string AuthParams()
        {
            return $"Name:{Name}/Role:{Role}/Email:{Email}/PasswordHash:{PasswordHash}";
        }

        public static User? ParseAuthParams(string userString)
        {
            var dictionary = new Dictionary<string, string>();

            string[] parts = userString.Split('/');

            foreach (string part in parts)
            {
                string[] keyValue = part.Split(':');

                if (keyValue.Length == 2)
                {
                    dictionary[keyValue[0]] = keyValue[1];
                }
            }

            try
            {
                return new User
                {
                    Name = dictionary["Name"],
                    Role = (UserRole)int.Parse(dictionary["Role"]),
                    Email = dictionary["Email"],
                    PasswordHash = dictionary["PasswordHash"]
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
