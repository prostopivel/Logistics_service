namespace Logistics_service.Models.Orders
{
    public class CustomerOrder : Order
    {
        public string BeginningAddress { get; set; }

        public string DestinationAddress { get; set; }

        public DateTime ArrivalTime { get; set; }

        public OrderStatus Status { get; set; }

        public string? Reason { get; set; }

        /// <summary>
        /// Конструктор по умолчанию.
        /// </summary>
        public CustomerOrder()
        {
            BeginningAddress = string.Empty;
            DestinationAddress = string.Empty;
        }
    }
}