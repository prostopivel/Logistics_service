using System.Drawing;

namespace Logistics_service.Models
{
    public class Manager : User
    {
        public List<Vehicle>? AssignedVehicles { get; set; }

        public void AssignVehicle(Customer customer, Vehicle vehicle)
        {
            // Логика назначения машины заказчику
        }

        public void UpdateRoute(Vehicle vehicle, List<Point> points)
        {
            // Логика обновления маршрута машины
        }

        public void NotifyUsers(string message)
        {
            // Логика отправки уведомлений заказчикам и администратору
        }
    }
}
