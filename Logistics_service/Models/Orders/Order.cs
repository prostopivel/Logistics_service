using System.ComponentModel.DataAnnotations;

namespace Logistics_service.Models.Orders
{
    public abstract class Order
    {
        public int Id { get; set; }

        public string? Email { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
