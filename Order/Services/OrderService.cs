using Cart.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Orders.Models;

namespace Orders.Services
{
    public class OrderService
    {
        private readonly IMongoCollection<OrderMaster> orderCollection;

        public OrderService(
            IOptions<OrderSetting> orderSetting)
        {

            //   HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
            //   Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))

            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") != null ? Environment.GetEnvironmentVariable("CONNECTION_STRING") : orderSetting.Value.ConnectionString;
            
            var mongoClient = new MongoClient(
                connectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                orderSetting.Value.DatabaseName);

            orderCollection = mongoDatabase.GetCollection<OrderMaster>(
                orderSetting.Value.OrdersCollectionName);
        }

        public async Task<List<OrderMaster>> GetAsync() =>
            await orderCollection.Find(_ => true).ToListAsync();

        public async Task<OrderMaster?> GetAsync(string id) =>
            await orderCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(OrderMaster order) =>
            await orderCollection.InsertOneAsync(order);

        public async Task UpdateAsync(string id, OrderMaster order) =>
            await orderCollection.ReplaceOneAsync(x => x.Id == id, order);

        public async Task RemoveAsync(string id) =>
            await orderCollection.DeleteOneAsync(x => x.Id == id);

    }
}
