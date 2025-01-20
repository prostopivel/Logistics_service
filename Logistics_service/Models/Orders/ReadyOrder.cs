using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics_service.Models.Orders
{
    public class ReadyOrder : Order, ICloneable
    {
        [Required(ErrorMessage = "CustomerEmail is required")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string CustomerEmail { get; set; }

        public int? RouteId { get; set; }

        [Required(ErrorMessage = "Route is required")]
        public virtual Route Route { get; set; }

        public int? VehicleId { get; set; }

        [Required(ErrorMessage = "Vehicle is required")]
        public virtual Vehicle Vehicle { get; set; }

        [Required(ErrorMessage = "ArrivalTime is required")]
        public DateTime ArrivalTime { get; set; }

        public ReadyOrderStatus? Status { get; set; }

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public ReadyOrder()
        {
            CustomerEmail = string.Empty;
            Route = new Route();
            Vehicle = new Vehicle();
        }

        /// <summary>
        /// Конструктор для инициализации заказа с маршрутом, транспортным средством, временем прибытия и электронной почтой клиента.
        /// </summary>
        /// <exception cref="ArgumentNullException">Выбрасывается, если route или vehicle равны null.</exception>
        public ReadyOrder(Route route, Vehicle vehicle, DateTime arrivalTime, string customerEmail)
        {
            Route = route ?? throw new ArgumentNullException(nameof(route));
            RouteId = route.Id;
            Vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
            VehicleId = vehicle.Id;
            ArrivalTime = arrivalTime;
            CustomerEmail = customerEmail ?? throw new ArgumentNullException(nameof(customerEmail));
            Status = ReadyOrderStatus.Created;
            SetTime(Vehicle.Speed);
        }

        /// <summary>
        /// Устанавливает время отправления на основе скорости транспортного средства.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void SetTime(int speed)
        {
            if (Route == null || Route.Distance == null)
            {
                throw new InvalidOperationException("Route and Distance must be set.");
            }

            var travelTime = TimeSpan.FromSeconds((int)(Route.Distance / speed));
            Route.DepartureTime = ArrivalTime - travelTime;
        }

        public object Clone()
        {
            return new ReadyOrder
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