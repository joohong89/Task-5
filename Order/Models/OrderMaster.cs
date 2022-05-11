using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Cart.Models
{
    public class OrderMaster
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public double Total { get; set; }

        // [INITIATED, SUCCESS, FAILED, CHECKOUT], 
        public string OrderStatus { get; set; }
        public List<CartDetails> Products { get; set; }
    }
}