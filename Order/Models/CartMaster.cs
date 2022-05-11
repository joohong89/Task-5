namespace Cart.Models
{
    public class CartMaster
    {
        public int Id { get; set; }
        public double Total { get; set; }
        public int OrderId { get; set; }

        // [ SUCCESS, FAILED], 
        public string OrderStatus { get; set; }
        public List<CartDetails> Products { get; set; }
    }
}