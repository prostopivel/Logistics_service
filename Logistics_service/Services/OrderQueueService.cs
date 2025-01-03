using Logistics_service.Models.Orders;
using System.Collections.Concurrent;

namespace Logistics_service.Services
{
    public class OrderQueueService<T> where T : Order
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private int id = 0;

        public void EnqueueOrder(T order)
        {
            order.Id = id++;
            _queue.Enqueue(order);
            Console.WriteLine($"Поступил заказ в {order.CreatedAt} от {order.Email}.");
        }

        public bool TryDequeueOrder(T order)
        {
            Console.WriteLine($"Принят заказ в {DateTime.Now} от {order.Email}.");
            return _queue.TryDequeue(out _);
        }

        public T[] PeekAll()
        {
            return _queue.ToArray();
        }
    }
}
