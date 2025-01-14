using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics_service.Models.Orders
{
    public class CustomerOrder : Order
    {
        [Required(ErrorMessage = "BeginningAddress is required")]
        public string BeginningAddress { get; set; }

        [Required(ErrorMessage = "DestinationAddress is required")]
        public string DestinationAddress { get; set; }

        [Required(ErrorMessage = "ArrivalTime is required")]
        public DateTime ArrivalTime { get; set; }

        public OrderStatus? Status { get; set; }

        public string? Reason { get; set; }
    }
}
