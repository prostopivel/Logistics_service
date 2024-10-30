namespace Logistics_service.Models
{
    public class Administrator : User
    {
        public void ViewAllOrders()
        {
            // Логика просмотра всех заказов
        }

        public void ProcessOrder(Order order)
        {
            // Логика обработки заказа
        }

        public void AddVehicle(Vehicle vehicle)
        {
            // Логика добавления новой машины в систему
        }

        public void ViewMap(DateTime date)
        {
            // Логика просмотра карты с маршрутами машин
        }
    }
}
