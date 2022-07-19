﻿using GeekShopping.MessageBus;
using GeekShopping.PaymentApi.Messages;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace GeekShopping.PaymentApi.RabbitMQSender
{
    public class RabbitMQMessageSender : IRabbitMQMessageSender
    {
        private readonly string _hostName;
        private readonly string _password;
        private readonly string _userName;
        private IConnection _connection;
        private const string _exchangeName = "DirectPaymentUpdateExchange";
        private const string _paymentEmailUpdateQueueName = "PaymentEmailUpdateQueueName";
        private const string _paymentOrderUpdateQueueName = "PaymentOrderUpdateQueueName";

        public RabbitMQMessageSender()
        {
            _hostName = "localhost";
            _password = "guest";
            _userName = "guest";
        }

        public void SendMessage(BaseMessage baseMessage)
        {
            if (ConnectionExists())
            {
                using var channel = _connection.CreateModel();
                channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct, durable: false);
                channel.QueueDeclare(_paymentEmailUpdateQueueName, false, false, false, null);
                channel.QueueDeclare(_paymentOrderUpdateQueueName, false, false, false, null);

                channel.QueueBind(_paymentEmailUpdateQueueName, _exchangeName, "PaymentEmail");
                channel.QueueBind(_paymentOrderUpdateQueueName, _exchangeName, "PaymentOrder");

                byte[] body = GetMessageAsByteArray(baseMessage);
                channel.BasicPublish(exchange: _exchangeName, "PaymentEmail", basicProperties: null, body: body);
                channel.BasicPublish(exchange: _exchangeName, "PaymentOrder", basicProperties: null, body: body);
            }
        }

        private byte[] GetMessageAsByteArray(BaseMessage baseMessage)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize<UpdatePaymentResultMessage>((UpdatePaymentResultMessage) baseMessage, options);
            var body = Encoding.UTF8.GetBytes(json);
            return body;
        }

        private bool ConnectionExists()
        {
            if (_connection != null)
                return true;

            CreateConnection();

            return _connection != null;
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    Password = _password,
                    UserName = _userName
                };

                _connection = factory.CreateConnection();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
