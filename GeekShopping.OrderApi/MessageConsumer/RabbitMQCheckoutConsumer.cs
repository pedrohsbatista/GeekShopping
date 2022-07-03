using GeekShopping.OrderApi.Messages;
using GeekShopping.OrderApi.Model;
using GeekShopping.OrderApi.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.OrderApi.MessageConsumer
{
    public class RabbitMQCheckoutConsumer : BackgroundService
    {
        private readonly OrderRepository _orderRepository;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQCheckoutConsumer(OrderRepository orderRepository)
        {
            _orderRepository = orderRepository;

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Password = "guest",
                UserName = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "checkoutQueue", false, false, false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (channel, evt) =>
            {
                var content = Encoding.UTF8.GetString(evt.Body.ToArray());
                CheckoutHeaderVO checkoutHeaderVO = JsonSerializer.Deserialize<CheckoutHeaderVO>(content);
                ProcessOrder(checkoutHeaderVO).GetAwaiter().GetResult();
                _channel.BasicAck(evt.DeliveryTag, false);
            };
            _channel.BasicConsume("checkoutQueue", false, consumer);
            return Task.CompletedTask;
        }

        private async Task ProcessOrder(CheckoutHeaderVO checkoutHeaderVO)
        {
            OrderHeader orderHeader = new()
            {
                 UserId = checkoutHeaderVO.UserId,
                 FirstName = checkoutHeaderVO.FirstName,
                 LastName = checkoutHeaderVO.LastName,
                 OrderDetails = new (),
                 CardNumber = checkoutHeaderVO.CardNumber,
                 CouponCode = checkoutHeaderVO.CouponCode,
                 Cvv = checkoutHeaderVO.Cvv,
                 DiscountAmount = checkoutHeaderVO.DiscountAmount,
                 Email = checkoutHeaderVO.Email,
                 ExpiryMonthYear = checkoutHeaderVO.ExpiryMonthYear,
                 OrderTime = DateTime.Now,
                 PurchaseAmount = checkoutHeaderVO.PurchaseAmount,
                 PaymentStatus = false,
                 Phone = checkoutHeaderVO.Phone,
                 PurchaseDate = checkoutHeaderVO.DateTime                 
            };

            foreach (var details in checkoutHeaderVO.CartDetails)
            {
                OrderDetail orderDetail = new ()
                {
                    ProductId = details.ProductId,
                    ProductName = details.Product.Name,
                    Price = details.Product.Price,
                    Count = details.Count
                };

                orderHeader.OrderTotalItens += orderDetail.Count;
                orderHeader.OrderDetails.Add(orderDetail);
            }

            await _orderRepository.AddOrder(orderHeader);
        }
    }
}
