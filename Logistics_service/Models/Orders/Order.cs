using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics_service.Models.Orders
{
    public abstract class Order
    {
        public int? Id { get; set; }

        public string? Email { get; set; }

        [NotMapped]
        public DateTime? CreatedAt { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is Order order && order.Id == Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Email, CreatedAt);
        }
    }
}
