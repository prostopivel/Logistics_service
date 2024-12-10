using System.ComponentModel.DataAnnotations;
using Logistics_service.Models.Users;

namespace Logistics_service.Models
{
    public class Notification
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Recipient is required")]
        public int RecipientId { get; set; }

        public User? Recipient { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(500, ErrorMessage = "Message cannot be longer than 500 characters")]
        public string Message { get; set; }

        public DateTime? SentAt { get; set; }

        public void Send()
        {
            // Логика отправки уведомления
        }
    }
}
