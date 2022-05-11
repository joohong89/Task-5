namespace Orders.Models
{
    public class OrderSetting: IOrderDatabaseSettings
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; } 
        public string OrdersCollectionName { get; set; } 
    }

    public interface IOrderDatabaseSettings
    {
        string OrdersCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
