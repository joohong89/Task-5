using Cart.Controllers;
using Cart.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Order.Consumer
{
    public class OrderProcessor: BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private CartMastersController cartMastersController;



        public OrderProcessor()
        {
          //  this._logger = loggerFactory.CreateLogger<OrderProcessor>();
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            var factory = new ConnectionFactory
            {

                 //HostName = "localhost" , 
                  //Port = 31672
                   HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                   Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))

            };

            // create connection  
            _connection = factory.CreateConnection();

            // create channel  
            _channel = _connection.CreateModel();

      
             _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                // received message  
                var content = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());

                // handle the received message  
                HandleMessageAsync(content);
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume("order-processed", false, consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessageAsync(string content)
        {
            // we just print this message   
            //_logger.LogInformation($"consumer received {content}");

            // convert to orders 
            OrderProcessQueue queue = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderProcessQueue>(content);

          

            // set Queue OBJ and save
            if (null != queue) {

                // get cart obj
                Microsoft.AspNetCore.Mvc.ActionResult<CartMaster> actionResult = await cartMastersController.GetCartMaster(queue.CartId);

                // set cart
                CartMaster master = actionResult.Value;
                master.OrderId = queue.OrderId;
                master.OrderStatus = queue.OrderStatus;

                // update db
                cartMastersController.PutCartMaster(master.Id, master);
              
              
            }

        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
