using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;

namespace TextRankCalc
{
    class Program
    {
        static readonly String BACKEND_API_EXCHANGE = "backend_api";
        static readonly String BACKEND_QUEUE = "text_rank_calc";
        static readonly String BACKEND_ROUTING_KEY = "TextCreated";
        static readonly String TEXTRANK_API_EXCHANGE = "textrank_api";
        static readonly String TEXTRANK_ROUTING_KEY = "TextRankTask";
        static readonly ConnectionFactory rabbit = new ConnectionFactory() { HostName = "localhost" };

        static void Main(string[] args)
        {
            using(var connection = rabbit.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(
                    exchange: BACKEND_API_EXCHANGE,
                    type: "direct"
                );
                channel.ExchangeDeclare(
                    exchange: TEXTRANK_API_EXCHANGE,
                    type: "direct"
                );
                var queue = channel.QueueDeclare(
                    queue: BACKEND_QUEUE,
                    durable: true,
                    exclusive: false,
                    autoDelete: false
                );
                channel.QueueBind(
                    queue: BACKEND_QUEUE,
                    exchange: BACKEND_API_EXCHANGE,
                    routingKey: BACKEND_ROUTING_KEY
                );

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish(
                        exchange: TEXTRANK_API_EXCHANGE,
                        routingKey: TEXTRANK_ROUTING_KEY,
                        basicProperties: properties,
                        body: body
                    );

                    channel.BasicAck(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false
                    );
                };
                channel.BasicConsume(
                    queue: BACKEND_QUEUE,
                    autoAck: false,
                    consumer: consumer
                );
                Console.WriteLine("Listening started");
                while (true)
                {
                    Console.ReadKey(true);
                }
            }
        }
    }
}
