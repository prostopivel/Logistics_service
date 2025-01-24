using Logistics_service.Models.Orders;

namespace Logistics_service.ViewModels.OrderModels
{
    public class CustomerOrderOutputModel
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? Email { get; set; }

        public string BeginningAddress { get; set; }

        public string DestinationAddress { get; set; }

        public DateTime ArrivalTime { get; set; }

        public OrderStatus Status { get; set; }

        public string Reason { get; set; }
    }
}
