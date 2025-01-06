using Logistics_service.Models.Orders;
using Logistics_service.Static;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace Logistics_service.Models
{
    public class Vehicle
    {
        [Key]
        public int? Id { get; set; }

        [Required(ErrorMessage = "Speed is required")]
        public int Speed { get; init; } = 15; // м/с

        [Required(ErrorMessage = "Garage is required")]
        public int GarageId { get; set; }

        public Point? Garage { get; set; }

        public VehicleStatus? Status { get; set; }

        [NotMapped]
        public int? PosX { get; set; }

        [NotMapped]
        public int? PosY { get; set; }

        [NotMapped]
        public double? CurrentDistance { get; private set; }

        [NotMapped]
        public Point? CurrentPoint { get; private set; }

        [NotMapped]
        public Route? CurrentRoute { get; private set; }

        [NotMapped]
        private SortedDictionary<DateTime, Route> _routes { get; init; }

        [NotMapped]
        public SortedDictionary<DateTime, Route> Routes { get => new SortedDictionary<DateTime, Route>(_routes); }

        public Vehicle()
        {
        }

        public Vehicle(Point point, int speed)
        {
            Garage = point;
            GarageId = point.Index;
            Status = new VehicleStatus();
            Speed = speed;
            Status = VehicleStatus.Free;
        }

        public Vehicle(Vehicle vehicle)
        {
            Id = vehicle.Id;
            Garage = (Point?)vehicle.Garage?.Clone() ?? new Point();
            GarageId = vehicle.GarageId;
            Speed = vehicle.Speed;
            CurrentPoint = (Point?)Garage?.Clone() ?? new Point();
            _routes = new SortedDictionary<DateTime, Route>();
            Status = vehicle.Status;
        }

        public bool SetOrder(ReadyOrder order)
        {
            if (_routes.Count(o => o.Key.Date.Date == order.ArrivalTime.Date) > 5)
            {
                Console.WriteLine("Превышен лимит заказов машины на день!");
                return false;
            }

            order.SetTime(Speed);

            foreach (var item in _routes)
            {
                if (order.Route.DepartureTime >= item.Value.DepartureTime
                    && order.ArrivalTime <= item.Key)
                {
                    Console.WriteLine("Данное время занято!");
                    return false;
                }
            }

            _routes.Add(order.ArrivalTime, order.Route);

            /*var firstElement = Routes.First();
            CurrentRoute = firstElement.Value;
            Routes.Remove(firstElement.Key);*/

            return true;
        }

        public void SetRoute(Point[] points)
        {
            if (CurrentRoute is null)
            {
                CurrentRoute = _routes.First().Value;
                _routes.Remove(_routes.First().Key);

                var tuple = DijkstraAlgorithm.FindShortestPath(points, CurrentPoint, CurrentRoute.DequeuePoint());

                CurrentRoute.AddPoints(tuple.Item1);
                CurrentRoute.Distance += tuple.Item2;
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is Vehicle vehicle && Id == vehicle.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Speed, GarageId);
        }

        private void SetDestination()
        {

        }

        public void UpdateLocation(int seconds)
        {

        }
    }
}