

namespace Cart.Models
{
    public class OrderProcessQueue
    {
        public string OrderId { get; set; }
        public int CartId { get; set; }

        // [INITIATED, SUCCESS, FAILED, CHECKOUT], 
        public string OrderStatus { get; set; }

    }
}