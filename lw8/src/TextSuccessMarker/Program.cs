using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;

namespace TextSuccessMarker
{
    class Program
    {
        static readonly String TEXTRANK_QUEUE = "text_success_marker";
        static readonly String TEXTRANK_API_EXCHANGE = "textrank_api";
        static readonly String TEXTRANK_ROUTING_KEY_IN = "TextRankCalculated";
        static readonly String TEXTRANK_ROUTING_KEY_OUT = "TextSuccessMarked";
        static readonly ConnectionFactory rabbit = new ConnectionFactory() { HostName = "localhost" };

        static readonly double MIN_SUCCESS_RANK = 0.5;

        static void Main(string[] args)
        {
            using(var connection = rabbit.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(
                    exchange: TEXTRANK_API_EXCHANGE,
                    type: "direct"
                );
                var queue = channel.QueueDeclare(
                    queue: TEXTRANK_QUEUE,
                    durable: true,
                    exclusive: false,
                    autoDelete: false
                );
                channel.QueueBind(
                    queue: TEXTRANK_QUEUE,
                    exchange: TEXTRANK_API_EXCHANGE,
                    routingKey: TEXTRANK_ROUTING_KEY_IN
                );

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    string[] data = Encoding.UTF8.GetString(body).Split(';');
                    string id = data[0];
                    double rank = double.Parse(data[1]);

                    string status = rank > MIN_SUCCESS_RANK ? "true" : "false";
                    string newBody = string.Format("{0};{1}", id, status);

                    channel.BasicPublish(
                        exchange: TEXTRANK_API_EXCHANGE,
                        routingKey: TEXTRANK_ROUTING_KEY_OUT,
                        basicProperties: properties,
                        body: Encoding.UTF8.GetBytes(newBody)
                    );

                    channel.BasicAck(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false
                    );
                };
                channel.BasicConsume(
                    queue: TEXTRANK_QUEUE,
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
