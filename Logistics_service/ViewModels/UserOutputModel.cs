using Logistics_service.Models.Users;
using System.Text.Json.Serialization;

namespace Logistics_service.ViewModels
{
    public class UserOutputModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserRole Role { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }
    }
}
