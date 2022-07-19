using GeekShopping.Email.Messages;
using GeekShopping.Email.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GeekShopping.Email.MessageConsumer
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private readonly EmailRepository _emailRepository;
        private IConnection _connection;
        private IModel _channel;
        private const string _exchangeName = "DirectPaymentUpdateExchange";
        private const string _paymentEmailUpdateQueueName = "PaymentEmailUpdateQueueName";

        public RabbitMQPaymentConsumer(EmailRepository emailRepository)
        {
            _emailRepository = emailRepository;
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct);
            _channel.QueueDeclare(_paymentEmailUpdateQueueName, false, false, false, null);
            _channel.QueueBind(_paymentEmailUpdateQueueName, _exchangeName, "PaymentEmail");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (channel, evt) =>
            {
                var content = Encoding.UTF8.GetString(evt.Body.ToArray());
                UpdatePaymentResultMessage updatePaymentResultMessage = JsonSerializer.Deserialize<UpdatePaymentResultMessage>(content);
                ProcessLog(updatePaymentResultMessage).GetAwaiter().GetResult();
                _channel.BasicAck(evt.DeliveryTag, false);
            };
            _channel.BasicConsume(_paymentEmailUpdateQueueName, false, consumer);
            return Task.CompletedTask;
        }

        private async Task ProcessLog(UpdatePaymentResultMessage updatePaymentResultMessage)
        {
            try
            {
                await _emailRepository.LogEmail(updatePaymentResultMessage);
            }
            catch(Exception)
            {
                throw;
            }
        }
    }
}
