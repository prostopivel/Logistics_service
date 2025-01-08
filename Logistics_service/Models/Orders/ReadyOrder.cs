using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics_service.Models.Orders
{
    public class ReadyOrder : Order, ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? DbId { get; set; }

        public string CustomerEmail { get; set; }

        public int? RouteId { get; set; }

        public Route Route { get; set; }

        public int? VehicleId { get; set; }

        public Vehicle Vehicle { get; set; }

        public DateTime ArrivalTime { get; set; }

        public ReadyOrder() { }

        public ReadyOrder(Route route, Vehicle vehicle, DateTime arrivalTime, string customerEmail)
        {
            Route = route;
            RouteId = route.Id;
            Vehicle = vehicle;
            VehicleId = vehicle.Id;
            ArrivalTime = arrivalTime;
            CustomerEmail = customerEmail;
            SetTime(15);

            if (!Vehicle.SetOrder(this))
            {
                throw new Exception("Превышен лимит заказов машины на день!");
            }
        }

        public void SetTime(int Speed)
        {
            var time = ArrivalTime;
            var dist = Route.Distance;

            var travelTime = TimeSpan.FromSeconds((int)(dist / Speed));
            Route.DepartureTime = time - travelTime;
        }

        public async Task OnTime()
        {

        }

        public object Clone()
        {
            return new ReadyOrder()
            {
                DbId = DbId,
                CustomerEmail = CustomerEmail,
                RouteId = RouteId,
                Route = Route,
                VehicleId = VehicleId,
                Vehicle = Vehicle,
                ArrivalTime = ArrivalTime
            };
        }
    }
}
