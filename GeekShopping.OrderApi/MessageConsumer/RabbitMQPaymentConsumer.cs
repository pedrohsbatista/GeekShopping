using GeekShopping.OrderApi.Messages;
using GeekShopping.OrderApi.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.OrderApi.MessageConsumer
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private readonly OrderRepository _orderRepository;
        private IConnection _connection;
        private IModel _channel;
        private const string _exchangeName = "DirectPaymentUpdateExchange";
        private const string _paymentOrderUpdateQueueName = "PaymentOrderUpdateQueueName";

        public RabbitMQPaymentConsumer(OrderRepository orderRepository)
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
            _channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct);
            _channel.QueueDeclare(_paymentOrderUpdateQueueName, false, false, false, null);
            _channel.QueueBind(_paymentOrderUpdateQueueName, _exchangeName, "PaymentOrder");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (channel, evt) =>
            {
                var content = Encoding.UTF8.GetString(evt.Body.ToArray());
                UpdatePaymentResultVO updatePaymentResultVO = JsonSerializer.Deserialize<UpdatePaymentResultVO>(content);
                UpdatePaymentStatus(updatePaymentResultVO).GetAwaiter().GetResult();
                _channel.BasicAck(evt.DeliveryTag, false);
            };
            _channel.BasicConsume(_paymentOrderUpdateQueueName, false, consumer);
            return Task.CompletedTask;
        }

        private async Task UpdatePaymentStatus(UpdatePaymentResultVO updatePaymentResultVO)
        {
            try
            {
                await _orderRepository.UpdateOrderPaymentStatus(updatePaymentResultVO.OrderId, updatePaymentResultVO.Status);
            }
            catch (Exception)
            {

            }
        }
    }
}
