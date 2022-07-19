using GeekShopping.PaymentApi.Messages;
using GeekShopping.PaymentApi.RabbitMQSender;
using GeekShopping.PaymentProcessor;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.PaymentApi.MessageConsumer
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly IProcessPayment _processPayment;
        private readonly IRabbitMQMessageSender _rabbitMQMessageSender;

        public RabbitMQPaymentConsumer(IProcessPayment processPayment,  IRabbitMQMessageSender rabbitMQMessageSender)
        {
            _processPayment = processPayment;
            _rabbitMQMessageSender = rabbitMQMessageSender;

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Password = "guest",
                UserName = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "orderPaymentProcessQueue", false, false, false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (channel, evt) =>
            {
                var content = Encoding.UTF8.GetString(evt.Body.ToArray());
                PaymentMessage paymentMessage = JsonSerializer.Deserialize<PaymentMessage>(content);
                ProcessPayment(paymentMessage).GetAwaiter().GetResult();
                _channel.BasicAck(evt.DeliveryTag, false);
            };
            _channel.BasicConsume("orderPaymentProcessQueue", false, consumer);
            return Task.CompletedTask;
        }

        private async Task ProcessPayment(PaymentMessage paymentMessage)
        {
            try
            {
                var result = _processPayment.PaymentProcessor();

                UpdatePaymentResultMessage updatePaymentResultMessage = new()
                {
                    Status = result,
                    OrderId = paymentMessage.OrderId,
                    Email = paymentMessage.Email
                };

                _rabbitMQMessageSender.SendMessage(updatePaymentResultMessage);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
