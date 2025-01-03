using Logistics_service.Models.Orders;
using Logistics_service.Static;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics_service.Models
{
    public class Vehicle
    {
        [Key]
        public int? Id { get; set; }

        [Required(ErrorMessage = "Garage is required")]
        public int GarageId { get; set; }

        public Point? Garage { get; set; }

        public VehicleStatus? Status { get; set; }

        [Required(ErrorMessage = "Speed is required")]
        public int Speed { get; init; } = 15; // м/с

        [NotMapped]
        public int? PosX { get; set; }

        [NotMapped]
        public int? PosY { get; set; }

        [NotMapped]
        public double CurrentDistance { get; private set; }

        [NotMapped]
        public Point CurrentPoint { get; private set; }

        [NotMapped]
        public Route? CurrentRoute { get; private set; }

        [NotMapped]
        private SortedDictionary<DateTime, Route> _routes { get; init; }

        [NotMapped]
        public SortedDictionary<DateTime, Route> Routes { get => new SortedDictionary<DateTime, Route>(_routes); }

        public Vehicle()
        {
            GarageId = 0;
            Status = VehicleStatus.Free;
            _routes = new SortedDictionary<DateTime, Route>();
            CurrentPoint = new Point() { Index = 0 };
            CurrentDistance = 0;
        }

        public Vehicle(Point garage, int speed) : this()
        {
            Garage = garage;
            GarageId = garage.Id;
            Speed = speed;
            CurrentPoint = Garage;
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

        private void SetDestination()
        {

        }

        public void UpdateLocation(int seconds)
        {

        }
    }
}