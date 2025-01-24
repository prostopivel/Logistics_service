using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics_service.Models.Orders
{
    public abstract class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Email { get; set; }

        public DateTime CreatedAt { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is Order order && Id == order.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Email, CreatedAt);
        }
    }
}