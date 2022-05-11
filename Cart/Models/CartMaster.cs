namespace Cart.Models
{
    public class CartMaster
    {
        public int Id { get; set; }
        public double Total { get; set; }
        public string OrderId { get; set; }

        // [INITIATED, SUCCESS, FAILED, CHECKOUT], 
        public string OrderStatus { get; set; }
        public List<CartDetails> Products { get; set; }
    }
}