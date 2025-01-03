namespace Logistics_service.Models.Orders
{
    public class ReadyOrder : Order
    {
        public string CustomerEmail { get; set; }

        public Route Route { get; set; }

        public Vehicle Vehicle { get; set; }

        public DateTime ArrivalTime { get; set; }

        public ReadyOrder(Route route, Vehicle vehicle, DateTime arrivalTime, string customerEmail)
        {
            Route = route;
            Vehicle = vehicle;
            ArrivalTime = arrivalTime;
            CustomerEmail = customerEmail;
            SetTime(15);

            if (!Vehicle.SetOrder(this))
            {
                throw new Exception("Превышен лимит заказов машины на день!");
            }
        }

        public void SetTime(int Speed)
        {
            var time = ArrivalTime;
            var dist = Route.Distance;

            var travelTime = TimeSpan.FromSeconds((int)(dist / Speed));
            Route.DepartureTime = time - travelTime;
        }

        public async Task OnTime()
        {

        }
    }
}
