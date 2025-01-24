using Logistics_service.Models.Orders;
using Logistics_service.Static;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics_service.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        public int Speed { get; init; } = 15;

        public int GarageId { get; set; }

        public Point Garage { get; set; }

        public VehicleStatus Status { get; set; }

        [NotMapped]
        public double PosX { get; set; }

        [NotMapped]
        public double PosY { get; set; }

        [NotMapped]
        public double CurrentDistance { get; private set; }

        [NotMapped]
        public Point? CurrentPoint { get; private set; }

        [NotMapped]
        public Route? CurrentRoute { get; private set; }

        [NotMapped]
        private SortedDictionary<DateTime, Route> _routes { get; init; } = new SortedDictionary<DateTime, Route>();

        [NotMapped]
        public SortedDictionary<DateTime, Route> Routes => new SortedDictionary<DateTime, Route>(_routes);

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public Vehicle() { }

        /// <summary>
        /// Конструктор для инициализации транспортного средства с начальной точкой и скоростью.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public Vehicle(Point point, int speed)
        {
            if (point == null)
            {
                throw new ArgumentNullException(nameof(point), "Начальная точка не может быть null.");
            }

            Garage = point;
            GarageId = point.Index;
            Speed = speed;
            Status = VehicleStatus.Free;
            CurrentPoint = (Point)Garage.Clone();
        }

        /// <summary>
        /// Конструктор для копирования транспортного средства.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public Vehicle(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                throw new ArgumentNullException(nameof(vehicle), "Транспортное средство не может быть null.");
            }

            Id = vehicle.Id;
            Garage = (Point)vehicle.Garage.Clone();
            GarageId = vehicle.GarageId;
            Speed = vehicle.Speed;
            CurrentPoint = (Point)vehicle.Garage.Clone();
            _routes = new SortedDictionary<DateTime, Route>();
            Status = vehicle.Status;
        }

        /// <summary>
        /// Устанавливает заказ для транспортного средства.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>True, если заказ успешно установлен, иначе False.</returns>
        public bool SetOrder(ReadyOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order), "Заказ не может быть null.");
            }

            order.SetTime(Speed);

            foreach (var item in _routes)
            {
                if (order.Route.DepartureTime >= item.Value.DepartureTime && order.ArrivalTime <= item.Key)
                {
                    Console.WriteLine("Данное время занято!");
                    return false;
                }
            }

            _routes.Add(order.ArrivalTime, order.Route);
            return true;
        }

        /// <summary>
        /// Устанавливает маршрут для транспортного средства.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetRoute(Point[] points)
        {
            if (points == null || points.Length == 0)
            {
                throw new ArgumentNullException(nameof(points), "Массив точек не может быть null или пустым.");
            }

            if (CurrentRoute == null && _routes.Count > 0)
            {
                CurrentRoute = _routes.First().Value;
                _routes.Remove(_routes.First().Key);

                var (path, distance) = DijkstraAlgorithm.FindShortestPath(points, CurrentPoint, CurrentRoute.DequeuePoint());

                CurrentRoute.AddPoints(path);
                CurrentRoute.Distance += distance;

                SetDestination();
            }
        }

        /// <summary>
        /// Устанавливает следующую точку назначения.
        /// </summary>
        private void SetDestination()
        {
            PosX = CurrentPoint.PosX;
            PosY = CurrentPoint.PosY;

            CurrentPoint = CurrentRoute?.DequeuePoint();
        }

        /// <summary>
        /// Обновляет местоположение транспортного средства.
        /// </summary>
        /// <returns>True, если местоположение обновлено, иначе False.</returns>
        public bool UpdateLocation(double time)
        {
            if (CurrentPoint == null)
            {
                return false;
            }

            double deltaX = (double)(CurrentPoint.PosX - PosX) * Point.ConvertX;
            double deltaY = (double)(CurrentPoint.PosY - PosY) * Point.ConvertY;

            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (distance <= 1)
            {
                SetDestination();
                return UpdateLocation(time);
            }

            double directionX = deltaX / distance;
            double directionY = deltaY / distance;

            double traveledDistance = Speed * time;

            if (traveledDistance >= distance)
            {
                SetDestination();
            }

            PosX += directionX * traveledDistance / Point.ConvertX;
            PosY += directionY * traveledDistance / Point.ConvertY;

            return true;
        }
    }
}