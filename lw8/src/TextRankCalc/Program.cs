using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;

namespace TextRankCalc
{
    class Program
    {
        static readonly String TEXTRANK_QUEUE = "text_rank_calc";
        static readonly String TEXTRANK_API_EXCHANGE = "textrank_api";
        static readonly String TEXTRANK_ROUTING_KEY_IN = "ProcessingAccepted";
        static readonly String TEXTRANK_ROUTING_KEY_OUT = "TextRankTask";
        static readonly ConnectionFactory rabbit = new ConnectionFactory() { HostName = "localhost" };
        static readonly String DB_PREFIX_STATUS = "-status";

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

                    var data = Encoding.UTF8.GetString(body).Split(';');
                    var status = data[1];

                    if (status == "true")
                    {
                        channel.BasicPublish(
                            exchange: TEXTRANK_API_EXCHANGE,
                            routingKey: TEXTRANK_ROUTING_KEY_OUT,
                            basicProperties: properties,
                            body: Encoding.UTF8.GetBytes(data[0])
                        );
                        SaveTextStatus(id);
                    }

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

        private static void SaveTextStatus(String contextId)
        {
            IDatabase db = redis.GetDatabase(GetDBNumber(contextId));
            db.StringSet(contextId + DB_PREFIX_STATUS, "processing");
        }
    }
}
