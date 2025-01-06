using Logistics_service.Models.Orders;
using System.Collections.Concurrent;

namespace Logistics_service.Services
{
    public class OrderQueueService<T> where T : Order
    {
        private readonly List<T> _orders = new List<T>();
        private readonly object _lock = new object();
        private int id = 0;

        public T[] Orders => _orders.ToArray();

        public void AddOrder(T order)
        {
            lock (_lock)
            {
                order.Id = id++;
                _orders.Add(order);
                Console.WriteLine($"Поступил заказ от {order.Email} в {order.CreatedAt}.");
            }
        }

        public bool TryDeleteOrder(T order)
        {
            if (_orders.Remove(order))
            {
                Console.WriteLine($"Заказ от {order.Email} принят в {DateTime.Now}.");
                return true;
            }

            Console.WriteLine($"Заказ от {order.Email} не принят в {DateTime.Now} про причине отсутствия в списке заказов!");
            return false;
        }

        public bool TryDeleteOrderById(int orderId, out T? order)
        {
            order = _orders.FirstOrDefault(o => o.Id == orderId);
            if (order is not null)
            {
                Console.WriteLine($"Заказ с Id {orderId} принят в {DateTime.Now}.");
                return _orders.Remove(order);
            }

            Console.WriteLine($"Заказ с Id {orderId} не принят в {DateTime.Now} про причине отсутствия в списке заказов!.");
            return false;
        }
    }
}
