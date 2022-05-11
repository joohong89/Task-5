using Cart.Models;
using Order.Models;
using Orders.Services;
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


        private readonly OrderService _orderService;

        public OrderProcessor(OrderService orderService)
        {
            _orderService = orderService;
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

            //_channel.ExchangeDeclare("demo.exchange", ExchangeType.Topic);

          
           // _channel.QueueDeclare("orders", true, false, false, null);
       
         
           
            // _channel.QueueBind("demo.queue.log", "demo.exchange", "demo.queue.*", null);
            // _channel.BasicQos(0, 1, false);

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

            _channel.BasicConsume("orders", false, consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessageAsync(string content)
        {
            // we just print this message   
            //_logger.LogInformation($"consumer received {content}");

            // convert to orders 
            CartMaster cartMaster = Newtonsoft.Json.JsonConvert.DeserializeObject<CartMaster>(content);
            OrderProcessQueue queueResponse = new OrderProcessQueue();
           

            // set Order OBJ and save
            if (null != cartMaster) {

                OrderMaster orderMaster = new OrderMaster();
                orderMaster.Id = Guid.NewGuid().ToString();
                orderMaster.Products = cartMaster.Products;
                orderMaster.Total = cartMaster.Total;

                // set queue
                queueResponse.OrderId = orderMaster.Id;
                queueResponse.CartId = cartMaster.Id;
                try
                {
                   
                

                    // TODO: got time change this 
                    orderMaster.OrderStatus = "SUCCESS";
                    // save item to db
                    await _orderService.CreateAsync(orderMaster);
                    //   OrderMaster? results = await _orderService.GetAsync(orderMaster.Id);

                    // send to queue status
                    queueResponse.OrderStatus = orderMaster.OrderStatus;
                    sendToQueue(queueResponse);
                  
                }
                catch (Exception ex) {
                    Console.WriteLine($"Order Failed");
                       

                    // if failed, send fail to queue
                    queueResponse.OrderStatus = "Failed";
                    sendToQueue(queueResponse);
                }
                
                // TODO: create success/fail scenario for carts service to picked up
          
            }
            /*
            string[] tokens = content.Split("=");
            string[] orders = tokens[1].Split(",");
            string[] customers = tokens[2].Split(",");
           // _logger.LogInformation($"after received {orders[0]}");
           // _logger.LogInformation($"Processing order... {orders[0]} ");

            await Task.Delay(30000);

          //  _logger.LogInformation($"will be published with updated orderid {orders[0]} and customer ID {customers[0]}");

            var factory = new ConnectionFactory()
            {
               // HostName = "localhost",
               // Port = 31672
                 HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                 Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))
            };

            Console.WriteLine(factory.HostName + ":" + factory.Port);
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "orderstatus",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                string message = "OrderId=" + orders[0] + ", CustomerId=" + customers[0] + ", status=Success";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: "orderstatus",
                                     basicProperties: null,
                                     body: body);
            }*/

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

        public void sendToQueue(OrderProcessQueue queueResponse) {

            _channel.QueueDeclare(queue: "order-processed",
                                   durable: false,
                                   exclusive: false,
                                   autoDelete: false,
                                   arguments: null);

            String res = Newtonsoft.Json.JsonConvert.SerializeObject(queueResponse);
            var body = Encoding.UTF8.GetBytes(res);

            _channel.BasicPublish(exchange: "",
                                 routingKey: "order",
                                 basicProperties: null,
                                 body: body);

        }
    }
}
