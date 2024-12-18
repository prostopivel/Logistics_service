using System.Drawing;

namespace Logistics_service.Models.Users
{
    public class Manager : User
    {
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

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
