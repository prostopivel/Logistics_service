using Logistics_service.Models;
using Logistics_service.Models.Orders;

namespace Logistics_service.ViewModels.OrderModels
{
    public class ReadyOrderOutputModel
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? Email { get; set; }

        public string CustomerEmail { get; set; }

        public RouteOutputModel Route { get; set; }

        public VehicleOutputModel Vehicle { get; set; }

        public DateTime ArrivalTime { get; set; }

        public ReadyOrderStatus Status { get; set; }
    }
}