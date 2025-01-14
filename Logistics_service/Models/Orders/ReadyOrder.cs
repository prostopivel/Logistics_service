using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics_service.Models.Orders
{
    public class ReadyOrder : Order, ICloneable
    {
        public string CustomerEmail { get; set; }

        public int? RouteId { get; set; }

        public virtual Route Route { get; set; }

        public int? VehicleId { get; set; }

        public virtual Vehicle Vehicle { get; set; }

        public DateTime ArrivalTime { get; set; }

        public ReadyOrderStatus? Status { get; set; }

        public ReadyOrder() { }

        public ReadyOrder(Route route, Vehicle vehicle, DateTime arrivalTime, string customerEmail)
        {
            Route = route;
            RouteId = route.Id;
            Vehicle = vehicle;
            VehicleId = vehicle.Id;
            ArrivalTime = arrivalTime;
            CustomerEmail = customerEmail;
            Status = ReadyOrderStatus.Created;
            SetTime(Vehicle.Speed);
        }

        public void SetTime(int Speed)
        {
            var time = ArrivalTime;
            var dist = Route.Distance;

            var travelTime = TimeSpan.FromSeconds((int)(dist / Speed));
            Route.DepartureTime = time - travelTime;
        }

        public object Clone()
        {
            return new ReadyOrder()
            {
                Id = Id,
                CustomerEmail = CustomerEmail,
                RouteId = RouteId,
                Route = new Route(Route),
                VehicleId = VehicleId,
                Vehicle = Vehicle,
                ArrivalTime = ArrivalTime,
                Status = Status,
                CreatedAt = CreatedAt,
                Email = Email
            };
        }
    }
}
