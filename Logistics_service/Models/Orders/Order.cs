using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics_service.Models.Orders
{
    public abstract class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public virtual string? Email { get; set; }

        public DateTime? CreatedAt { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is not Order order)
            {
                return false;
            }

            return Id == order.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Email, CreatedAt);
        }
    }
}